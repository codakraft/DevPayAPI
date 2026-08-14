# Connecting to ProvidusBank over an Azure Site-to-Site VPN

A reproduction guide. Everything here was verified against the live DevPayAPI
deployment; parameter values were read back from the running Azure resources on
2026-08-12 rather than transcribed from notes.

The goal is that someone starting a second Providus integration can follow this
end to end without rediscovering the things that cost us days. Those are
collected in [§10 Pitfalls](#10-pitfalls-that-cost-real-time) — read that section
first if you are debugging rather than building.

---

## 1. What this achieves, and when you need it

ProvidusBank exposes its fund-transfer API on a private address, `192.168.156.27:8888`,
reachable only through an IPsec tunnel. They also operate a firewall that permits
traffic from **one specific source address** — a `/32` you nominate.

That single sentence drives the entire design. If your application runs anywhere
that produces a *range* of source addresses — Azure App Service, containers,
serverless, autoscaled VMs — you cannot satisfy a `/32` permit directly, and you
need the egress proxy described here.

**Timeline expectation:** the Azure build is a few hours. Getting the bank's
firewall permit and reconciling IPsec parameters took **just over three weeks**
of correspondence. Plan for the bank being the critical path, and start the
paperwork on day one.

---

## 2. Before you touch Azure: what to obtain from the bank

Ask for all of this **in one message**. Each round trip costs days.

| What to ask | Why it matters |
|---|---|
| Their VPN peer public IP | Goes in the Local Network Gateway |
| The private IP **and port** of the service | This is your encryption domain and your `BaseUrl` |
| Is the tunnel **route-based (VTI)** or **policy-based (crypto map)**? | Determines one critical Azure toggle — see §6 |
| Their exact crypto ACL / encryption domain | Must match yours *exactly* or Phase 2 fails |
| Full Phase 1 and Phase 2 parameters | Encryption, integrity, DH group, lifetimes |
| Whether they NAT your traffic | Changes what source address they see |
| Which source `/32` they will permit, and **on which ports** | ICMP and TCP are separate permits |
| A named network engineer with a phone number | Email alone will not resolve a live tunnel issue |

Providus sends a **B2B VPN Request Form** (a PDF) to be completed. §7 covers how
to fill it, including three fields where the obvious answer is wrong.

---

## 3. Architecture

```
App Service devpayProd   (VNet integration; sources from 10.0.0.0/24)
        │  http://10.0.2.4:8888
        ▼
Proxy VM devpay-providus-proxy   10.0.2.4   (nginx reverse proxy)
        │  source is always 10.0.2.4  ←── the /32 Providus permits
        ▼
VPN Gateway devpay-vpngw   4.253.3.57
        │  IPsec tunnel; encryption domain 10.0.0.0/16 ⇄ 192.168.156.27/32
        ▼
ProvidusBank   102.209.190.3   →   192.168.156.27:8888
```

**The proxy VM exists for exactly one reason: to provide a single stable source
IP.** It performs no business function. App Service VNet integration assigns
outbound addresses dynamically from its integration subnet, so without the proxy
the bank would see a rotating set of sources and their `/32` permit would match
almost nothing.

### Why not VPN Gateway NAT instead of a VM?

Azure supports NAT rules on the VPN Gateway, which would collapse all sources to
one address without a VM. We ruled it out:

1. NAT rules require **VpnGw2 or higher** — a permanent SKU upgrade.
2. NAT rules are **unsupported on connections using policy-based traffic
   selectors** — which Providus's Cisco requires.

Those two are mutually exclusive with our requirements, so the VM is not a
workaround, it is the only option. A `Standard_B1s` costs a fraction of the SKU
upgrade that would not have worked anyway.

---

## 4. Resource inventory

Tenant *Simplicity International Services Ltd* · subscription
`1c52cba0-d953-4542-8ae2-c4aa3f450830` · RG `prodGroup` · region **South Africa North**

| Resource | Configuration |
|---|---|
| VNet `devpay-vnet` | `10.0.0.0/16` |
| — subnet `default` | `10.0.0.0/24`, delegated to `Microsoft.Web/serverFarms` |
| — subnet `GatewaySubnet` | `10.0.1.0/24` — **must** be named exactly this |
| — subnet `proxy-subnet` | `10.0.2.0/24` |
| Gateway `devpay-vpngw` | VpnGw1AZ, Generation2, **RouteBased**, BGP off, active-active off |
| Public IP `devpay-vpngw-ip` | `4.253.3.57` — our peer address |
| LNG `providus-lng` | peer `102.209.190.3`, prefix `192.168.156.27/32` |
| Connection `devpay-connection` | IKEv2, PSK, **policy-based traffic selectors ON**, DPD 45s |
| VM `devpay-providus-proxy` | Standard_B1s, Ubuntu 22.04 LTS, static `10.0.2.4`, **no public IP** |
| NSG `devpay-proxy-nsg` | on `proxy-subnet` — see §5.5 |
| App Service `devpayProd` | S1 Windows, .NET 8, VNet-integrated to `devpay-vnet/default` |
| Action group `devpay-vpn-alerts` | email notifications |

Region choice matters: put the gateway in the region closest to the bank. South
Africa North is the nearest Azure region to Nigeria and keeps round-trip latency
tolerable for synchronous disbursement calls.

---

## 5. Build

### 5.1 Network foundation

```bash
RG=prodGroup
LOC=southafricanorth
VNET=devpay-vnet

az group create --name "$RG" --location "$LOC"

az network vnet create \
  --resource-group "$RG" --name "$VNET" --location "$LOC" \
  --address-prefixes 10.0.0.0/16 \
  --subnet-name default --subnet-prefixes 10.0.0.0/24

# The gateway subnet MUST be named exactly "GatewaySubnet".
# Azure matches on the literal name; any other name fails at gateway creation.
# /27 is the practical minimum; /24 leaves room for future gateway SKUs.
az network vnet subnet create \
  --resource-group "$RG" --vnet-name "$VNET" \
  --name GatewaySubnet --address-prefixes 10.0.1.0/24
```

Do **not** attach an NSG to `GatewaySubnet`. Azure requires specific management
traffic to reach the gateway, and an NSG there is a well-known way to break a
tunnel in ways that are hard to diagnose.

### 5.2 VPN Gateway

```bash
az network public-ip create \
  --resource-group "$RG" --name devpay-vpngw-ip \
  --location "$LOC" --sku Standard --allocation-method Static --zone 1 2 3

# Takes 30-45 minutes. Start it, then do the paperwork in §7 while it builds.
az network vnet-gateway create \
  --resource-group "$RG" --name devpay-vpngw \
  --location "$LOC" --vnet "$VNET" \
  --public-ip-addresses devpay-vpngw-ip \
  --gateway-type Vpn --vpn-type RouteBased \
  --sku VpnGw1AZ --vpn-gateway-generation Generation2 \
  --no-wait
```

**Create the gateway as RouteBased even though the bank runs policy-based.** This
is not a contradiction and it is the single most misunderstood point in the whole
build. Azure's legacy `PolicyBased` gateway type is a different, far more limited
product (IKEv1 only, one tunnel, no coexistence). Interoperating with a
policy-based peer is done with a **RouteBased gateway plus the policy-based
traffic selector toggle** on the connection — §5.4.

Record the public IP; the bank needs it:

```bash
az network public-ip show -g "$RG" -n devpay-vpngw-ip --query ipAddress -o tsv
```

### 5.3 Local Network Gateway

The LNG describes the *far* side: their peer IP and the address space reachable
through the tunnel.

```bash
az network local-gateway create \
  --resource-group "$RG" --name providus-lng --location "$LOC" \
  --gateway-ip-address 102.209.190.3 \
  --local-address-prefixes 192.168.156.27/32
```

**The prefix must match their crypto ACL exactly.** We initially used
`192.168.156.0/24`; Phase 1 came up and Phase 2 never did, with no useful error
anywhere. Narrowing to `192.168.156.27/32` fixed it immediately. See §10.

> If the bank ever adds a second endpoint, this prefix must be updated or the new
> host is silently unreachable — no error, just timeouts.

### 5.4 The connection

```bash
az network vpn-connection create \
  --resource-group "$RG" --name devpay-connection --location "$LOC" \
  --vnet-gateway1 devpay-vpngw \
  --local-gateway2 providus-lng \
  --shared-key '<PSK>' \
  --connection-protocol IKEv2

# Pin the crypto to exactly what was agreed. Without an explicit policy Azure
# offers a broad default proposal set, which a strict Cisco peer may reject.
az network vpn-connection ipsec-policy add \
  --resource-group "$RG" --connection-name devpay-connection \
  --ike-encryption AES256 --ike-integrity SHA256 --dh-group DHGroup14 \
  --ipsec-encryption AES256 --ipsec-integrity SHA256 --pfs-group PFS2048 \
  --sa-lifetime 3600 --sa-max-size 102400000

# THE CRITICAL TOGGLE for a policy-based (crypto map) peer.
az network vpn-connection update \
  --resource-group "$RG" --name devpay-connection \
  --set usePolicyBasedTrafficSelectors=true
```

Verified live configuration:

```json
{
  "protocol": "IKEv2",
  "policyBasedTS": true,
  "dpd": 45,
  "ikev2": [{
    "dhGroup": "DHGroup14",
    "ikeEncryption": "AES256",  "ikeIntegrity": "SHA256",
    "ipsecEncryption": "AES256","ipsecIntegrity": "SHA256",
    "pfsGroup": "PFS2048",
    "saLifeTimeSeconds": 3600,  "saDataSizeKilobytes": 102400000
  }]
}
```

Agreed parameters:

| | Phase 1 (IKEv2) | Phase 2 (IPsec) |
|---|---|---|
| Encryption | AES256 (CBC) | AES256 (CBC) |
| Integrity | SHA256 | SHA256 |
| DH / PFS group | DHGroup14 (2048-bit MODP) | PFS2048 (= group 14) |
| Lifetime | **28800 s — fixed by Azure** | 3600 s / 102400000 KB |
| Encapsulation | UDP 500, NAT-T 4500 | ESP, tunnel mode |
| Authentication | Pre-shared key | — |

**Azure's Phase 1 lifetime is hard-coded at 28800 s and cannot be set.** Providus
asked for 86400. Each peer rekeys on its own timer so a mismatch is normally
tolerated, but say so explicitly up front — a Cisco peer *can* be configured to
be strict about it, and you do not want to discover that during debugging.

AH is not used and not supported. ESP provides integrity; if their form asks for
"Authentication Only (AH)", the answer is **No**.

### 5.5 Proxy subnet, NSG, and VM

Verbatim from `build-proxy.sh`, which built the live proxy:

```bash
RG=prodGroup; LOC=southafricanorth; VNET=devpay-vnet
SUBNET=proxy-subnet; SUBNET_CIDR=10.0.2.0/24
NSG=devpay-proxy-nsg; VM=devpay-providus-proxy; PRIVATE_IP=10.0.2.4
APP_SUBNET_CIDR=10.0.0.0/24

az network vnet subnet create \
  --resource-group "$RG" --vnet-name "$VNET" \
  --name "$SUBNET" --address-prefixes "$SUBNET_CIDR"

az network nsg create --resource-group "$RG" --name "$NSG" --location "$LOC"

# Only the App Service integration subnet may reach the proxy port.
az network nsg rule create \
  --resource-group "$RG" --nsg-name "$NSG" --name allow-appservice-8888 \
  --priority 100 --direction Inbound --access Allow --protocol Tcp \
  --source-address-prefixes "$APP_SUBNET_CIDR" --source-port-ranges '*' \
  --destination-address-prefixes "$PRIVATE_IP" --destination-port-ranges 8888

# Everything else inbound from the VNet is denied.
az network nsg rule create \
  --resource-group "$RG" --nsg-name "$NSG" --name deny-other-vnet-inbound \
  --priority 4000 --direction Inbound --access Deny --protocol '*' \
  --source-address-prefixes VirtualNetwork --source-port-ranges '*' \
  --destination-address-prefixes '*' --destination-port-ranges '*'

az network vnet subnet update \
  --resource-group "$RG" --vnet-name "$VNET" --name "$SUBNET" \
  --network-security-group "$NSG"

az vm create \
  --resource-group "$RG" --name "$VM" --location "$LOC" \
  --image "Canonical:0001-com-ubuntu-server-jammy:22_04-lts-gen2:latest" \
  --size Standard_B1s \
  --vnet-name "$VNET" --subnet "$SUBNET" \
  --private-ip-address "$PRIVATE_IP" \
  --public-ip-address "" \
  --nsg "" \
  --admin-username azureuser --generate-ssh-keys \
  --custom-data cloud-init-proxy.yaml
```

Note `--public-ip-address ""` and `--nsg ""`. **The VM has no public IP and no
inbound internet path, by design** — it sits in the payment path. Manage it with
`az vm run-command invoke` or the portal Serial Console. Do not attach a public
IP for convenience.

The static private IP is essential: it is the `/32` the bank permits. A dynamic
address would eventually change and silently break disbursements.

Return-path rules, added once the bank could reach us:

| Priority | Name | Access | Proto | Source | Port |
|---|---|---|---|---|---|
| 100 | allow-appservice-8888 | Allow | Tcp | 10.0.0.0/24 | 8888 |
| 300 | allow-providus-icmp | Allow | Icmp | 192.168.156.27/32 | * |
| 310 | allow-providus-8888 | Allow | Tcp | 192.168.156.27/32 | 8888 |
| 4000 | deny-other-vnet-inbound | Deny | * | VirtualNetwork | * |

> Azure's `VirtualNetwork` service tag **includes on-premises ranges reachable
> through a local network gateway**. The bank's traffic therefore matches the
> broad deny at 4000 unless explicitly allowed above it. This is not obvious and
> produced a confusing round of one-way connectivity.

### 5.6 nginx on the proxy

First boot installs a plain L4 TCP forward via cloud-init:

```yaml
#cloud-config
package_update: true
packages:
  - nginx
  - libnginx-mod-stream

write_files:
  - path: /etc/nginx/stream.d/providus.conf
    permissions: '0644'
    content: |
      server {
          listen 8888;
          proxy_pass 192.168.156.27:8888;
          proxy_connect_timeout 10s;
          proxy_timeout 300s;   # matches Providus:TimeoutSeconds in the app
      }

runcmd:
  - mkdir -p /etc/nginx/stream.d
  - |
    grep -q 'stream.d' /etc/nginx/nginx.conf || \
      printf '\nstream {\n    include /etc/nginx/stream.d/*.conf;\n}\n' >> /etc/nginx/nginx.conf
  - nginx -t
  - systemctl enable nginx
  - systemctl restart nginx
```

Allow about two minutes after `az vm create` before testing.

We later replaced this with an **HTTP reverse proxy** (`switch-to-http-proxy.sh`)
so that each request is logged with status and timing. When a bank says "we see
no traffic from you", per-request logs on your own hop end the argument:

```nginx
log_format providus '$time_iso8601 client=$remote_addr method=$request_method uri=$request_uri '
                    'status=$status upstream=$upstream_addr upstream_status=$upstream_status '
                    'req_time=$request_time upstream_time=$upstream_response_time '
                    'req_bytes=$request_length resp_bytes=$body_bytes_sent';

server {
    listen 8888;
    server_name _;

    access_log /var/log/nginx/providus_access.log providus;
    error_log  /var/log/nginx/providus_error.log warn;

    client_max_body_size 10m;

    location / {
        proxy_pass http://192.168.156.27:8888;
        proxy_http_version 1.1;

        # Host is left as the upstream so the bank sees exactly what a direct
        # call would have sent, not the proxy address.
        proxy_set_header Host $proxy_host;
        # Deliberately NO X-Forwarded-For — it would disclose internal App
        # Service instance addressing to the bank, who has no use for it.

        proxy_connect_timeout 10s;
        proxy_send_timeout    300s;
        proxy_read_timeout    300s;

        proxy_buffering off;
    }
}
```

Two deliberate choices worth preserving:

- **Log metadata only, never bodies.** Request bodies contain the API password
  and the debit account number. The log format above records status, timing and
  sizes — enough to prove a request happened, nothing sensitive.
- **Timeouts match the application.** `proxy_read_timeout 300s` mirrors
  `Providus:TimeoutSeconds`. A proxy that gives up before the app does converts a
  slow-but-successful transfer into an ambiguous one, which for a payment is the
  worst outcome — see §11.

Confirm log rotation is covered: `grep '/var/log/nginx/\*.log' /etc/logrotate.d/nginx`.

### 5.7 App Service VNet integration

Integrate the app into the `default` subnet so its outbound traffic enters the
VNet and can reach `10.0.2.4`:

```bash
az webapp vnet-integration add \
  --resource-group "$RG" --name devpayProd \
  --vnet "$VNET" --subnet default
```

The subnet must be delegated to `Microsoft.Web/serverFarms` and cannot be shared
with anything else.

> If a NAT Gateway is attached to that subnet, it does **not** interfere: the
> tunnel route to `192.168.156.27/32` is more specific and wins. Ours
> (`devpay-nat`) predates this work and was left alone.

### 5.8 Application cutover

Only after the TCP test in §8 returns `OPEN`:

```jsonc
"Providus": {
  "BaseUrl": "http://10.0.2.4:8888",   // was http://154.113.166.30:5120
  "TimeoutSeconds": 300
}
```

**Both host and port change.** The public endpoint and the VPN endpoint are
different services on different ports — confirm the API contract is identical
before assuming a drop-in swap. In our case the VPN endpoint exposed a
*superset*: the NIP inter-bank endpoints existed there and we had not been using
them. Enumerate what is available on the private endpoint rather than assuming
parity.

---

## 6. Route-based vs policy-based: the decision that matters most

Ask the bank one question — *"is your tunnel route-based (VTI) or policy-based
(crypto map)?"* — and let the answer drive this:

| Their side | Your Azure config |
|---|---|
| Route-based (VTI) | RouteBased gateway, `usePolicyBasedTrafficSelectors` **false** |
| **Policy-based (crypto map)** | RouteBased gateway, `usePolicyBasedTrafficSelectors` **true** ← Providus |

A route-based Azure gateway proposes wildcard traffic selectors (`0.0.0.0/0 ⇄
0.0.0.0/0`). A Cisco crypto map expects selectors matching its ACL exactly and
will reject the wildcard proposal — **after Phase 1 has already succeeded**.

That failure mode is genuinely nasty: the portal shows the connection as not
connected, with no indication that IKE authentication actually worked fine. You
can burn days re-checking the PSK. The tell is `MmsaCount = 1` with
`QmsaCount = 0` — see §9.

---

## 7. Filling in the bank's VPN request form

Providus sends a PDF with two columns; you complete the client side. Three
fields have non-obvious answers.

| Field | Value | Note |
|---|---|---|
| Remote Network Gateway | your gateway public IP | `4.253.3.57` |
| Firewall (VPN Server) | Azure VPN Gateway (VpnGw1AZ, route-based) | |
| Private IP (Local IP) | **the subnet you want whitelisted** | see below |
| Customer Port | N/A — outbound only | |
| Phase 1 — IKEv1 block | **leave entirely blank** | IKEv2 only |
| Pre-shared key | **leave blank** | see below |

**Never write the PSK on the form.** Send it through a separate channel and ask
them to read theirs back character by character. A PSK mismatch and a traffic
selector mismatch are the two most likely causes of a tunnel that never comes up,
and they are hard to tell apart from the Azure side.

**Private IP: give the subnet you want permitted, not the whole VNet.** We
initially wrote `10.0.0.0/16` (the encryption domain). What they actually needed
to whitelist was narrower. Once the proxy exists, the answer is simply
`10.0.2.4/32`.

**State explicitly that the `/32` is a firewall permit, not a crypto selector.**
This one distinction cost us the most time. Azure derives policy-based traffic
selectors from VNet address prefixes and **cannot propose a `/32`**. The bank's
network team may read a `/32` request as a demand that your encryption domain be
a `/32`, which Azure cannot do. Spell out the difference in every message:

> Our encryption domain is `10.0.0.0/16`. Separately, all traffic reaching you
> will be sourced from the single host `10.0.2.4` — that is the address for your
> firewall permit.

Also confirm on the form: the service **port** (theirs said 8888 while our app
was configured for 5120), the Phase 1 lifetime discrepancy (§5.4), and the
contact email spelling — a typo there means missed tunnel notifications.

---

## 8. Verification, in order

Each rung isolates one layer. Do not skip upward.

```bash
# 1. Tunnel status
az network vpn-connection show -g prodGroup -n devpay-connection \
  --query "{status:connectionStatus, in:ingressBytesTransferred, out:egressBytesTransferred}" -o json

# 2. Phase 1 / Phase 2 SA counts
GWID=$(az network vnet-gateway show -g prodGroup -n devpay-vpngw --query id -o tsv)
az monitor metrics list --resource "$GWID" --metric MmsaCount QmsaCount \
  --interval PT5M --offset 30m --aggregation Average -o json

# 3. Go/no-go: can we actually reach the service?
az vm run-command invoke -g prodGroup -n devpay-providus-proxy --command-id RunShellScript \
  --scripts "timeout 8 bash -c '</dev/tcp/192.168.156.27/8888' && echo OPEN || echo CLOSED" \
  --query "value[0].message" -o tsv

# 4. Application-layer probe (credential-free, read-only)
az vm run-command invoke -g prodGroup -n devpay-providus-proxy --command-id RunShellScript \
  --scripts "curl -s -o /dev/null -w 'HTTP %{http_code}\n' --max-time 20 \
             http://192.168.156.27:8888/postingrest/GetNIPBanks" \
  --query "value[0].message" -o tsv

# 5. Packet capture — settles which side is dropping
az vm run-command invoke -g prodGroup -n devpay-providus-proxy --command-id RunShellScript \
  --scripts "timeout 20 tcpdump -ni eth0 -c 40 host 192.168.156.27" --query "value[0].message" -o tsv
```

Interpreting step 3:

| Result | Meaning |
|---|---|
| `OPEN` | Reachable. Proceed. |
| Silence / timeout | A firewall is **dropping** packets — usually theirs |
| Connection refused (RST) | Packets arrive; the port is closed or nothing is listening |
| No route to host | Routing problem on **your** side |

**Never conclude reachability from `ping`.** ICMP and TCP are permitted by
separate firewall rules. Providus pinged us successfully for roughly 17 hours
while TCP 8888 remained blocked. A successful ping proves the tunnel carries
traffic and nothing more.

Step 5 is what ends disagreements. If your SYN leaves the interface and nothing
comes back, the drop is downstream of you, and a `tcpdump` transcript is evidence
the bank's network team will act on where prose will not.

---

## 9. Monitoring

Azure surfaces almost nothing useful about tunnel health in the portal. These
metric alerts are the only reliable signal.

| Alert | Condition | Sev | Meaning |
|---|---|---|---|
| `devpay-vpn-tunnel-down` | `QmsaCount < 1` | 1 | Phase 2 SA lost — **disbursements will fail** |
| `devpay-vpn-phase1-down` | `MmsaCount < 1` | 2 | IKE broken — PSK, proposal, or peer unreachable |
| `devpay-vpn-ts-mismatch` | TS-mismatch drops > 0 | 2 | Encryption domains disagree |

`MmsaCount` counts Phase 1 SAs, `QmsaCount` counts Phase 2 SAs.

**Phase 1 up with Phase 2 down (`MmsaCount=1`, `QmsaCount=0`) is the signature of
a traffic-selector or transform mismatch.** Azure exposes this state nowhere in
the portal — `connectionStatus` reflects Phase 2 only, so a working IKE
negotiation with failing IPsec looks identical to a completely dead tunnel. These
metrics are the only way to distinguish them. Healthy steady state is
`MmsaCount=1, QmsaCount=1`.

---

## 10. Pitfalls that cost real time

1. **The `/32` is a firewall permit, not the crypto selector.** Azure cannot
   propose a `/32` traffic selector; it derives selectors from VNet prefixes.
   Being explicit about this distinction in every message avoids days of
   cross-purposes. *(Days lost: several.)*

2. **Phase 2 failed until the LNG prefix exactly matched their crypto ACL.**
   `192.168.156.0/24` → `192.168.156.27/32` fixed it. Symptom: Phase 1 up,
   Phase 2 down, no useful error anywhere.

3. **Azure never exposes Phase 1 state.** Use `MmsaCount` / `QmsaCount`. Without
   them you cannot tell "PSK wrong" from "selectors wrong".

4. **`VirtualNetwork` service tag includes on-premises ranges** reachable via a
   local network gateway. A broad deny rule silently blocks the bank's return
   traffic until you add explicit allows above it.

5. **VPN Gateway NAT is not an option alongside policy-based selectors** — and
   also needs VpnGw2+. Check both constraints before designing around NAT.

6. **Diagnose by failure mode, not by ping.** Silence means dropping; RST means
   closed; ICMP success proves nothing about TCP.

7. **Azure's Phase 1 lifetime is fixed at 28800 s.** Raise it early rather than
   letting it surface as an unexplained rekey failure.

8. **The private endpoint may not be the same service as the public one.** Ours
   exposed additional endpoints we did not know existed. Enumerate before
   cutting over.

9. **Do not put an NSG on `GatewaySubnet`.** Azure needs management traffic
   through it.

10. **Secrets leak through convenience.** Our PSK was displayed in Cloud Shell
    and must now be rotated as a coordinated change with the bank. Treat the PSK
    as write-only from the moment it is set.

---

## 11. Known risks

- **Single point of failure.** One proxy VM; a host maintenance reboot stops
  disbursements. The `/32` constraint forces this — two VMs behind an internal
  load balancer would still present two source addresses. Mitigation: ask the
  bank to permit a second `/32` and run an active/passive pair.
- **PSK rotation outstanding.** Exposed 2026-08-03; rotation requires
  coordination with the bank's network team.
- **Credentials travel as cleartext HTTP inside the tunnel.** IPsec protects the
  wire; this is not end-to-end encryption. The API password and debit account
  number are in every request body. Never log bodies on the proxy.
- **Ambiguous timeouts are a financial risk, not just an availability one.** If a
  transfer request times out you cannot know whether the debit landed. The
  application must use a stable, persisted transaction reference so the bank's
  duplicate detection can recognise a retry, and must requery rather than
  re-sending. See the disbursement idempotency handling in `LoanService` and
  `ProvidusDisbursementService`.

---

## 12. Reproduction checklist

- [ ] Obtain everything in §2 from the bank in one request
- [ ] Confirm route-based vs policy-based (§6)
- [ ] Create VNet, `default` subnet, `GatewaySubnet` (no NSG on it)
- [ ] Create public IP and VPN Gateway — start early, takes 30-45 min
- [ ] Send the bank your gateway public IP
- [ ] Create LNG with their peer IP and their **exact** crypto ACL prefix
- [ ] Create connection: IKEv2, explicit IPsec policy, PSK via a separate channel
- [ ] Set `usePolicyBasedTrafficSelectors=true` if their peer is policy-based
- [ ] Create proxy subnet, NSG, and VM with a **static private IP, no public IP**
- [ ] Give the bank the proxy `/32`, stating it is a firewall permit not a selector
- [ ] Confirm which **ports** they permit (TCP *and* ICMP are separate)
- [ ] Add NSG allow rules for their prefix above the broad deny
- [ ] Integrate App Service into the VNet
- [ ] Verify `MmsaCount=1, QmsaCount=1`
- [ ] Verify TCP reachability returns `OPEN` — not ping
- [ ] Probe an application endpoint through the tunnel
- [ ] Enumerate what the private endpoint actually exposes
- [ ] Cut the application over to the proxy address
- [ ] Create the three metric alerts (§9)
- [ ] Confirm nginx log rotation
- [ ] Rotate the PSK if it was ever displayed anywhere

---

## 13. Contacts and source files

**ProvidusBank:** Abiodun Abayomi — networkadmin@providusbank.com — 07039814410
**Devtage:** Adedamola Agunbiade — 08035566830

| File | Purpose |
|---|---|
| `build-proxy.sh` | Creates subnet, NSG, and proxy VM from scratch |
| `cloud-init-proxy.yaml` | First-boot config — installs nginx, L4 forward |
| `switch-to-http-proxy.sh` | Converts L4 forward to the logging HTTP reverse proxy |
| `RUNBOOK.md` | Day-to-day operational runbook for this deployment |

Providus API documentation: https://developer.providusbank.com/transfer-services
