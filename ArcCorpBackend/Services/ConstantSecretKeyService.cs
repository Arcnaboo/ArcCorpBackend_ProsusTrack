namespace ArcCorpBackend.Services
{
    public class ConstantSecretKeyService
    {
        private static readonly Guid ApiKeyGuid = new Guid("2b150884-be96-4854-85b8-d7e63101ca46");
        private static readonly string EncryptedJWTKey = "E|E,^|e7.ETTuT[u-^eV.V[E7E\\,d-[\\...-|euH^<^|<EH|[7VV<d-u-\\d\\[.Tu7^V||VTT-E,|e\\e\\EE,^H[^,,d,|H-TT|TV|7[E^7T-.7dEH^7,uVuu,[T|,|\\\\|";
        private readonly string JWTKey;
        public static readonly ConstantSecretKeyService Instance = new ConstantSecretKeyService();


        private ConstantSecretKeyService()
        {
            var enigma = new Enigma3Service();
            JWTKey = enigma.Decrypt(ApiKeyGuid, EncryptedJWTKey);
        

        
        }

        public string GetJWT()
        {
            return JWTKey;
        }
        
    }
}
