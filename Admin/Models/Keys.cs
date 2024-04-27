namespace Admin.Models
{
    public interface IKeys
    {
        public byte[] EncryptionKey { get; }
        public byte[] TokenKey { get; }
        public string ElasticKey { get; }
        public string HashIdsKey { get; }
        public string CaptchaKey { get;}
    }
    public class Keys: IKeys
    {
        public Keys(byte[] encryptionKey, byte[] tokenKey, string elasticKey, string hashShiftWorkKey, string captchaKey)
        {
            EncryptionKey = encryptionKey;
            TokenKey = tokenKey;
            ElasticKey = elasticKey;
            HashIdsKey = hashShiftWorkKey;
            CaptchaKey = captchaKey;
        }
        public byte[] EncryptionKey { get; }
        public byte[] TokenKey { get; }
        public string ElasticKey { get; }
        public string HashIdsKey { get; }
        public string CaptchaKey { get; }
    }
}