using Galileo.DataBaseTier;

namespace Galileo.BusinessLogic
{
    public class BDAnalisisBL
    {

        protected BDAnalisisBL(IConfiguration config)
        {
        }

        public static List<string> TablasCargar(string name)
        {
            return BDAnalisisDB.TablasCargar(name);
        }
    }
}
