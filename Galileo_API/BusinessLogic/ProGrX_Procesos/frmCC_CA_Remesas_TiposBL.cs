using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.DataBaseTier.ProGrX.Procesos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Procesos
{
    public class FrmCccARemesasTiposBL
    {
        private readonly FrmCccARemesasTiposDB _db;

        public FrmCccARemesasTiposBL(IConfiguration config)
        {
            _db = new FrmCccARemesasTiposDB(config);
        }

        public ErrorDto<CcCaRemesasTiposLista> RemesasTipos_Lista_Obtener(
            int CodEmpresa,
            string jfiltros,
            string entidad)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)
                              ?? new FiltrosLazyLoadData();

                return _db.RemesasTipos_Lista_Obtener(CodEmpresa, filtros, entidad);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CcCaRemesasTiposLista>(
                    ex.Message,
                    -1,
                    new CcCaRemesasTiposLista());
            }
        }

        public ErrorDto<List<CcCaRemesasTiposData>> RemesasTipos_Obtener(
            int CodEmpresa,
            string jfiltros,
            string entidad)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros)
                              ?? new FiltrosLazyLoadData();

                return _db.RemesasTipos_Obtener(CodEmpresa, filtros, entidad);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcCaRemesasTiposData>>(ex.Message);
            }
        }

        public ErrorDto RemesasTipos_Guardar(
            int CodEmpresa,
            string usuario,
            CcCaRemesasTiposData item)
        {
            return _db.RemesasTipos_Guardar(CodEmpresa, usuario, item);
        }

        public ErrorDto RemesasTipos_Eliminar(
            int CodEmpresa,
            int id,
            string usuario)
        {
            return _db.RemesasTipos_Eliminar(CodEmpresa, id, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> RemesasTipos_Entidades_Obtener(int CodEmpresa)
        {
            return _db.RemesasTipos_Entidades_Obtener(CodEmpresa);
        }
    }
}