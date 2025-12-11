using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasProcesoBL
    {
        private readonly FrmActivosObrasProcesoDB _db;

        public FrmActivosObrasProcesoBL(IConfiguration config)
        {
            _db = new FrmActivosObrasProcesoDB(config);
        }
        public ErrorDto Activos_Obras_Actualizar(int CodEmpresa, string estado, DateTime fecha_finiquito, string contrato)
        {
            return _db.Activos_Obras_Actualizar(CodEmpresa, estado, fecha_finiquito, contrato);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTipos_Obtener(int CodEmpresa)
        {
            return _db.Activos_ObrasTipos_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_ObrasTiposDesem_Obtener(int CodEmpresa)
        {
            return _db.Activos_ObrasTiposDesem_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obras_Obtener(int CodEmpresa)
        {
            return _db.Activos_Obras_Obtener(CodEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Obra_Proveedores_Obtener(int CodEmpresa)
        {
            return _db.Activos_Obra_Proveedores_Obtener(CodEmpresa);
        }
        public ErrorDto<ActivosObrasData?> Activos_Obras_Consultar(int CodEmpresa, string contrato)
        {
            return _db.Activos_Obras_Consultar(CodEmpresa, contrato);
        }
        public ErrorDto<List<ActivosObrasProcesoAdendumsData>> Activos_ObrasAdendums_Obtener(int CodEmpresa, string contrato, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Activos_ObrasAdendums_Obtener(CodEmpresa, contrato, filtros);
        }
        public ErrorDto<List<ActivosObrasProcesoDesembolsosData>> Activos_ObrasDesembolsos_Obtener(int CodEmpresa, string contrato, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Activos_ObrasDesembolsos_Obtener(CodEmpresa, contrato, filtros);
        }
        public ErrorDto<List<ActivosObrasProcesoResultadosData>> Activos_ObrasResultados_Obtener(int CodEmpresa, string contrato)
        {
            return _db.Activos_ObrasResultados_Obtener(CodEmpresa, contrato);
        }
        public ErrorDto Activos_Obras_Modificar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            return _db.Activos_Obras_Modificar(CodEmpresa, data, usuario);
        }
        public ErrorDto Activos_Obras_Insertar(int CodEmpresa, ActivosObrasData data, string usuario)
        {
            return _db.Activos_Obras_Insertar(CodEmpresa, data, usuario);
        }
        public ErrorDto Activos_Obra_Eliminar(int CodEmpresa, string contrato, string usuario)
        {
            return _db.Activos_Obra_Eliminar(CodEmpresa, contrato, usuario);
        }
        public ErrorDto Activos_ObrasAdendum_Guardar(int CodEmpresa, ActivosObrasProcesoAdendumsData dato, string usuario, string contrato, decimal addendums, decimal presu_actual)
        {
            return _db.Activos_ObrasAdendum_Guardar(CodEmpresa, dato, usuario, contrato, addendums, presu_actual);
        }
         public ErrorDto Activos_ObrasDesembolso_Guardar(int CodEmpresa, ActivosObrasProcesoDesembolsosData dato, string usuario, string contrato)
        {
            return _db.Activos_ObrasDesembolso_Guardar(CodEmpresa, dato, usuario, contrato);
        }
    }
}
