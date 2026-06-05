using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrx_Personas
{
    public class FrmAfSuspendidosGestionBl
    {
        private readonly FrmAfSuspendidosGestionDb DbAfSuspendidosGestion;

        public FrmAfSuspendidosGestionBl(IConfiguration config)
        {
            DbAfSuspendidosGestion = new FrmAfSuspendidosGestionDb(config);
        }

        public ErrorDto<List<AfSuspendidosBitacoraDto>> AF_Suspendidos_Bitacora_Obtener(int CodEmpresa, string Filtro)
        {
            AfSuspendidosGestionFiltros filtros = JsonConvert.DeserializeObject<AfSuspendidosGestionFiltros>(Filtro) ?? new AfSuspendidosGestionFiltros();
            return DbAfSuspendidosGestion.AF_Suspendidos_Bitacora_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_Suspendidos_Gestion_Registrar(int CodEmpresa, string Cedula, int Accion, string Notas, string Usuario)
        {
            return DbAfSuspendidosGestion.AF_Suspendidos_Gestion_Registrar(CodEmpresa, Cedula, Accion, Notas, Usuario);
        }

        public ErrorDto<List<AfSuspendidosArchivoDto>> AF_Suspendidos_Archivo_Cargar(int CodEmpresa, int Valor, string Usuario, string Lista)
        {
            List<AfSuspendidosArchivoDto> lista = JsonConvert.DeserializeObject<List<AfSuspendidosArchivoDto>>(Lista) ?? new List<AfSuspendidosArchivoDto>();
            return DbAfSuspendidosGestion.AF_Suspendidos_Archivo_Cargar(CodEmpresa, Valor, Usuario, lista);
        }

        public ErrorDto AF_Suspendidos_Archivo_Procesar(int CodEmpresa, int Valor, string Usuario)
        {
            return DbAfSuspendidosGestion.AF_Suspendidos_Archivo_Procesar(CodEmpresa, Valor, Usuario);
        }

        public ErrorDto<TablasListaGenericaModel> AF_Suspendidos_Personas_Obtener(int CodEmpresa, string jfiltro)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltro) ?? new FiltrosLazyLoadData();
            return DbAfSuspendidosGestion.AF_Suspendidos_Personas_Obtener(CodEmpresa, filtro);
        }
    }
}
