using Galileo.DataBaseTier;

using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class BDAnalisisBL
    {

        private readonly IConfiguration _config;

        public BDAnalisisBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<string>> TablasCargar()
        {
            var db = new BDAnalisisDB(_config);
            return db.TablasCargar();
        }

        public ErrorDto<List<Dictionary<string, object?>>> ResultadosObtener(string objeto)
        {
            return new BDAnalisisDB(_config).ResultadosObtener(objeto);
        }

        public ErrorDto<List<BDAnalisisEstructuraDto>> EstructuraObtener(string objeto)
        {
            return new BDAnalisisDB(_config).EstructuraObtener(objeto);
        }
    }
}
