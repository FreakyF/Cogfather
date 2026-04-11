using OtpNet;

namespace Cogfather.HQ.Infrastructure.Identity;

public class TotpService
{
    public string GenerateSecret()
    {
        var secret = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secret);
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "Cogfather")
    {
        return $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
    }

    public bool VerifyTotp(string secret, string code)
    {
        var bytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(bytes);
        return totp.VerifyTotp(code, out var timeWindowUsed, new VerificationWindow(2, 2));
    }
}