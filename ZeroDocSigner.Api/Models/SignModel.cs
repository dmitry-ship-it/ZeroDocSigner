using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Api.Models
{
    public class SignModel: BaseModel
    {
        public SignatureParameters Parameters { get; set; }

        public bool Force { get; set; }
    }
}
