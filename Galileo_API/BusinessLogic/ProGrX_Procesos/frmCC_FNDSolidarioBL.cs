using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCcFndSolidarioBL
    {
        private readonly FrmCcFndSolidarioDB _db;

        public FrmCcFndSolidarioBL(IConfiguration config)
        {
            _db = new FrmCcFndSolidarioDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FNDSolidario_Instituciones_Obtener(int codEmpresa)
                => _db.FNDSolidario_Instituciones_Obtener(codEmpresa);

        public ErrorDto FrmCC_FNDSolidario_Ejecutar(int codEmpresa, string usuario, int codContabilidad, int codInstitucion)
                     => _db.FrmCC_FNDSolidario_Ejecutar(codEmpresa, usuario, codContabilidad, codInstitucion);

    }
}
