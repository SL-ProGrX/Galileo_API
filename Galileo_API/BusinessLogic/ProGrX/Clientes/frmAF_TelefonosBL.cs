using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFTelefonosBL
    {
        private readonly FrmAFTelefonosDB _db;

        public FrmAFTelefonosBL(IConfiguration config)
        {
            _db = new FrmAFTelefonosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposTelefonos_Obtener(int CodEmpresa)
        {
            return _db.AF_TiposTelefonos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfTelefonosDto>> AF_Telefonos_ObtenerPorCedula(int CodEmpresa, string cedula)
        {
            return _db.AF_Telefonos_ObtenerPorCedula(CodEmpresa, cedula);
        }

        public ErrorDto AF_Telefono_Insertar(int CodEmpresa, string cedula, int tipoId, string numero, string ext, string contacto, string usuario)
        {
            return _db.AF_Telefono_Insertar(CodEmpresa, cedula, tipoId, numero, ext, contacto, usuario);
        }

        public ErrorDto AF_Telefono_Actualizar(int CodEmpresa, int telefonoId, int tipoId, string numero, string ext, string contacto, string usuario)
        {
            return _db.AF_Telefono_Actualizar(CodEmpresa, telefonoId, tipoId, numero, ext, contacto, usuario);
        }

        public ErrorDto AF_Telefono_Eliminar(int CodEmpresa, int telefonoId)
        {
            return _db.AF_Telefono_Eliminar(CodEmpresa, telefonoId);
        }
    }
}