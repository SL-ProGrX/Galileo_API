using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivProfesionalesBL
    {
        private readonly FrmVivProfesionalesDB _db;

        public FrmVivProfesionalesBL(IConfiguration config)
        {
            _db = new FrmVivProfesionalesDB(config);
        }

        public ErrorDto<List<VivContactoDto>> VivContactos_Lista(int codEmpresa, VivContactoFiltroParams filtro)
            => _db.VivContactos_Lista(codEmpresa, filtro);

        public ErrorDto<List<DropDownListaGenericaModel>> VivTiposId_Lista(int codEmpresa)
            => _db.VivTiposId_Lista(codEmpresa);

        public ErrorDto<List<CrdSgtBancoDto>> CrdSgtBancos_Lista(int codEmpresa, CrdSgtBancoParams param)
            => _db.CrdSgtBancos_Lista(codEmpresa, param);

        public ErrorDto<List<VivCuentaBancariaDto>> VivCuentasBancarias_Lista(int codEmpresa, string identificacion)
            => _db.VivCuentasBancarias_Lista(codEmpresa, identificacion);

        public ErrorDto<CrdVivContactoConsultaDto> CrdVivContacto_Consulta(int codEmpresa, int idContacto)
            => _db.CrdVivContacto_Consulta(codEmpresa, idContacto);
    }
}
