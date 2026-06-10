using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrRemesasCreditoBL
    {
        private readonly FrmCrRemesasCreditoDB DB;

        public FrmCrRemesasCreditoBL(IConfiguration config)
        {
            DB = new FrmCrRemesasCreditoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Fuente_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_RemesasCredito_Fuente_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Estado_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_RemesasCredito_Estado_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_RemesasCredito_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Usuarios_Dropdown_Obtener(int CodEmpresa, string? codGrupo)
        {
            return DB.CR_RemesasCredito_Usuarios_Dropdown_Obtener(CodEmpresa, codGrupo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Destinos_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_RemesasCredito_Destinos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_DestinosLinea_Dropdown_Obtener(int CodEmpresa, string? codigo)
        {
            return DB.CR_RemesasCredito_DestinosLinea_Dropdown_Obtener(CodEmpresa, codigo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Oficinas_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_RemesasCredito_Oficinas_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_RemesasCredito_Tags_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_RemesasCredito_Tags_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<CrRemesasCreditoLista> CR_RemesasCredito_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_RemesasCredito_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrRemesasCreditoLista> CR_RemesasCredito_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_RemesasCredito_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrRemesasCreditoCrearResult> CR_RemesasCredito_Crear(int CodEmpresa, CrRemesasCreditoCrearRequest request)
        {
            return DB.CR_RemesasCredito_Crear(CodEmpresa, request);
        }

        public ErrorDto<CrRemesasCreditoTagLista> CR_RemesasCredito_Tags_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_RemesasCredito_Tags_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrRemesasCreditoTagLista> CR_RemesasCredito_Tags_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_RemesasCredito_Tags_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_RemesasCredito_Tags_Guardar(int CodEmpresa, CrRemesasCreditoTagGuardarRequest request)
        {
            return DB.CR_RemesasCredito_Tags_Guardar(CodEmpresa, request);
        }

        public ErrorDto<CrRemesasCreditoInformeLista> CR_RemesasCredito_Informes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_RemesasCredito_Informes_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrRemesasCreditoInformeLista> CR_RemesasCredito_Informes_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_RemesasCredito_Informes_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrRemesasCreditoArchivoDigitalDto> CR_RemesasCredito_ArchivoDigital_Consultar(int CodEmpresa, int remesa)
        {
            return DB.CR_RemesasCredito_ArchivoDigital_Consultar(CodEmpresa, remesa);
        }

        public ErrorDto CR_RemesasCredito_ArchivoDigital_Recibir(int CodEmpresa, CrRemesasCreditoArchivoDigitalRequest request)
        {
            return DB.CR_RemesasCredito_ArchivoDigital_Recibir(CodEmpresa, request);
        }

        public ErrorDto<CrRemesasCreditoConsultaDto> CR_RemesasCredito_Consulta_Operacion_Obtener(int CodEmpresa, long operacion)
        {
            return DB.CR_RemesasCredito_Consulta_Operacion_Obtener(CodEmpresa, operacion);
        }

        public ErrorDto<CrRemesasCreditoListadoCargaResult> CR_RemesasCredito_Listados_Cargar(int CodEmpresa, CrRemesasCreditoListadoCargaRequest request)
        {
            return DB.CR_RemesasCredito_Listados_Cargar(CodEmpresa, request);
        }

        public ErrorDto<CrRemesasCreditoListadoCargaResult> CR_RemesasCredito_Listados_Export(int CodEmpresa, CrRemesasCreditoListadoCargaRequest request)
        {
            return DB.CR_RemesasCredito_Listados_Export(CodEmpresa, request);
        }

        public ErrorDto<CrRemesasCreditoReporteDto> CR_RemesasCredito_Reporte_Datos_Obtener(int CodEmpresa, CrRemesasCreditoReporteRequest request)
        {
            return DB.CR_RemesasCredito_Reporte_Datos_Obtener(CodEmpresa, request);
        }
    }
}