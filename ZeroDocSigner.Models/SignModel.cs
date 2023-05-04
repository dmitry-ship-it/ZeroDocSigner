namespace ZeroDocSigner.Models;

public class SignModel : DataModel
{
    public bool Force { get; set; }

    public SignerModel SignerInfo { get; set; }
}
