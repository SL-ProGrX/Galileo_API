using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFTelemarketingConsultasBL
    {
        private readonly FrmAFTelemarketingConsultasDB _db;

        public FrmAFTelemarketingConsultasBL(IConfiguration config)
        {
            _db = new FrmAFTelemarketingConsultasDB(config);
        }

        #region Colocacion

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Categoria_Obtener(int CodEmpresa)
        {
            return _db.AF_Telemarketing_Categoria_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfTelemarketingColocacionData>> AF_Telemarketing_Colocacion_Obtener(int CodEmpresa, string filtros)
        {
            ColocacionFiltros colocacion = JsonConvert.DeserializeObject<ColocacionFiltros>(filtros) ?? new ColocacionFiltros();
            return _db.AF_Telemarketing_Colocacion_Obtener(CodEmpresa, colocacion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Catalogos_Obtener(int CodEmpresa, string tipo)
        {
            return _db.AF_Telemarketing_Catalogos_Obtener(CodEmpresa, tipo);
        }

        #endregion

        #region Clientes

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_Lineas_Obtener(int CodEmpresa, int combo)
        {
            return _db.AF_Telemarketing_Lineas_Obtener(CodEmpresa, combo);
        }

        public ErrorDto<List<AfTelemarketingClientesData>> AF_Telemarketing_Clientes_Obtener(int CodEmpresa, ClientesFiltros filtros)
        {
            return _db.AF_Telemarketing_Clientes_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<AfTelemarketingClientesDetalleData>> AF_Telemarketing_ClientesDetalle_Obtener(int CodEmpresa, string vCadena, string usuario)
        {
            return _db.AF_Telemarketing_ClientesDetalle_Obtener(CodEmpresa, vCadena, usuario);
        }

        #endregion

        #region Contactos

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Telemarketing_EstadosPer_Obtener(int CodEmpresa)
        {
            return _db.AF_Telemarketing_EstadosPer_Obtener(CodEmpresa);
        }

        public ErrorDto<List<AfTelemarketingContactoData>> AF_Telemarketing_Contacto_Obtener(int CodEmpresa, string filtros)
        {
            ContactosFiltros contacto = JsonConvert.DeserializeObject<ContactosFiltros>(filtros) ?? new ContactosFiltros();
            return _db.AF_Telemarketing_Contacto_Obtener(CodEmpresa, contacto);
        }

        #endregion
    }
}