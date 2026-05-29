using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFBitacoraEspecialBL
    {
        private readonly FrmAFBitacoraEspecialDB _db;

        public FrmAFBitacoraEspecialBL(IConfiguration config)
        {
            _db = new FrmAFBitacoraEspecialDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_BitacoraEspecialMov_Obtener(int CodEmpresa)
        {
            return _db.AF_BitacoraEspecialMov_Obtener(CodEmpresa);
        }

        public ErrorDto AF_BitacoraEspecial_Revisar(int CodEmpresa, string usuario, List<AFBitacoraEspecialData> bitacora)
        {
            return _db.AF_BitacoraEspecial_Revisar(CodEmpresa, usuario, bitacora);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_BitacoraEspecialBusquedas_Obtener(int CodEmpresa, string campo)
        {
            return _db.AF_BitacoraEspecialBusquedas_Obtener(CodEmpresa, campo);
        }

        public ErrorDto<List<AFBitacoraEspecialData>> AF_BitacoraEspecial_Obtener(int CodEmpresa, string jFiltros)
        {
            AFBitacoraEspecialFiltros filtros = JsonConvert.DeserializeObject<AFBitacoraEspecialFiltros>(jFiltros) ?? new AFBitacoraEspecialFiltros();
            return _db.AF_BitacoraEspecial_Obtener(CodEmpresa, filtros);
        }
    }
}