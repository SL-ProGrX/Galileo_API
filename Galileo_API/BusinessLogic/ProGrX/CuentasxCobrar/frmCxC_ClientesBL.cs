using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesBL
    {
        private readonly FrmCxCClientesDB _db;

        public FrmCxCClientesBL(IConfiguration config)
        {
            _db = new FrmCxCClientesDB(config);
        }

        public ErrorDto<List<CxcPersonaDto>> CxcPersonas_Lista(int codEmpresa, string orden)
        {
            return _db.CxcPersonas_Lista(codEmpresa, orden);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> EstadoCivil_Lista(int codEmpresa)
        {
            return _db.EstadoCivil_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Clasificacion_Lista(int codEmpresa)
        {
            return _db.Clasificacion_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposId_Lista(int codEmpresa)
        {
            return _db.TiposId_Lista(codEmpresa);
        }
    }
}
