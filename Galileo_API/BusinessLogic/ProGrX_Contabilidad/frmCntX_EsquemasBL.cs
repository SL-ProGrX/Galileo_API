using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXEsquemasBl
    {
        private readonly FrmCntXEsquemasDB _db;

        public FrmCntXEsquemasBl(IConfiguration config)
        {
            _db = new FrmCntXEsquemasDB(config);
        }

        public ErrorDto Copiar(int codEmpresa,int codFuente,int codDestino,bool inicializa,string usuario )
        {
            return _db.Copiar(codEmpresa,codFuente,codDestino,inicializa,usuario);
        }

        public ErrorDto<List<ContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            return _db.ObtenerContabilidades(codEmpresa);
        }


    }
}
