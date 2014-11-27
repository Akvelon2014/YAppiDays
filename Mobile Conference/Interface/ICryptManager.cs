namespace MobileConference.Interface
{
    public interface ICryptManager
    {
        string GetHash(string inputData);
        bool VerifyHash(string inputData, string storingHash);
    }
}
