namespace LendingSolution.Core.Dtos.Response.Remita;

public class MandateResponse : ResponseBase
{
    public MandateData Data { get; set; } // Replace with real type if known
}

public class MandateData
{
    public string Statuscode { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string MandateId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

}

public class ValidateMandateResponse
{

}

public class DebitInstructionResponse
{

}

public class StopMandateResponse
{

}