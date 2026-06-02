namespace Cogfather.HQ.Infrastructure.Identity;

public class CaptchaService
{
    public (string Question, int Answer) GenerateCaptcha()
    {
        var random = new Random();
        var a = random.Next(1, 10);
        var b = random.Next(1, 10);
        return ($"What is {a} + {b}?", a + b);
    }

    public bool VerifyCaptcha(int expectedAnswer, int userAnswer)
    {
        return expectedAnswer == userAnswer;
    }
}