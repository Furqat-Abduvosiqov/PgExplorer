namespace Pg.Explorer.Shared.Encryptions;

public interface IEncryptionService
{
    public string EncryptPassword(string password);
    public string DecryptPassword(string encryptedPassword);
}