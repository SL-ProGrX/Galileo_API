using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPrendasMonitorDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrPrendasMonitorDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de prenda activos para el monitor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_TiposPrenda_Obtener(int codEmpresa)
        {
            const string SqlTiposPrenda = @"
                select
                    rtrim(tipo_prenda) as item,
                    rtrim(descripcion) as descripcion
                from crd_prendas_tipos
                where activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlTiposPrenda
            );
        }

        /// <summary>
        /// Obtiene la lista de catalogo segun tipo para el monitor de prendas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_Catalogo_Obtener(int codEmpresa, string tipo)
        {
            const string SqlCatalogo = @"
                exec spCrd_Prendas_Cat_List_Cbo
                    @Tipo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlCatalogo,
                new
                {
                    Tipo = (tipo ?? string.Empty).Trim()
                }
            );
        }

        /// <summary>
        /// Obtiene la lista de estados de persona.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_EstadosPersona_Obtener(int codEmpresa)
        {
            const string SqlEstadosPersona = @"
                select
                    rtrim(cod_estado) as item,
                    rtrim(descripcion) as descripcion
                from afi_estados_persona;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlEstadosPersona
            );
        }

        /// <summary>
        /// Obtiene la lista de unidades activas segun el tipo de aplicacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_UnidadesCilindraje_Obtener(int codEmpresa, string tipo)
        {
            string tipoFiltro = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (tipoFiltro != "CIL" && tipoFiltro != "CAP" && tipoFiltro != "PE")
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "El parametro tipo debe ser CIL, CAP o PE.",
                    Result = []
                };
            }

            const string SqlUnidadesCilindraje = @"
                select
                    rtrim(id_unidad) as item,
                    rtrim(descripcion) as descripcion
                from crd_prendas_uds
                where (
                        (@Tipo = 'CIL' and cilindraje_apl = 1)
                     or (@Tipo = 'CAP' and capacidad_apl = 1)
                     or (@Tipo = 'PE' and peso_apl = 1)
                )
                  and activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlUnidadesCilindraje,
                new
                {
                    Tipo = tipoFiltro
                }
            );
        }
    }
}
