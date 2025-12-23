using Galileo.DataBaseTier;

namespace Galileo.BusinessLogic
{
    public class BDAnalisisBL
    {

        private readonly IConfiguration _config;

        public BDAnalisisBL(IConfiguration config)
        {
            _config = config;
        }

        public List<string> TablasCargar()
        {
            var db = new BDAnalisisDB(_config);
            return db.TablasCargar();
        }
    }
}
