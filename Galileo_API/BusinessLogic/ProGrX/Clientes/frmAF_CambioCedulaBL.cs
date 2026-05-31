using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic
{
    public class FrmAFCambioCedulaBL
    {
        private readonly FrmAfCambioCedulaDB _db;

        public FrmAFCambioCedulaBL(IConfiguration config)
        {
            _db = new FrmAfCambioCedulaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposCedulas_Obtener(int CodEmpresa)
        {
            return _db.AF_TiposCedulas_Obtener(CodEmpresa);
        }

        public ErrorDto AF_CambioCedula_Aplicar(int CodEmpresa, string usuario, string cambioCedula)
        {
            return _db.AF_CambioCedula_Aplicar(CodEmpresa, usuario, cambioCedula);
        }

        public ErrorDto<AFCedulaCambioDto> AF_Cedula_Obtener(int CodEmpresa, string cedula)
        {
            return _db.AF_Cedula_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto AF_Usuario_Validar(int CodEmpresa, string parametros)
        {
            return _db.AF_Usuario_Validar(CodEmpresa, parametros);
        }
    }
}