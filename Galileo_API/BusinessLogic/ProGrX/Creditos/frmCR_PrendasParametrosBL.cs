using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPrendasParametrosBL
    {
        private readonly FrmCrPrendasParametrosDB DB;

        public FrmCrPrendasParametrosBL(IConfiguration config)
        {
            DB = new FrmCrPrendasParametrosDB(config);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasCatalogoData>> CR_PrendasParametros_Catalogo_Lista_Obtener(int CodEmpresa, string tipo)
        {
            return DB.CR_PrendasParametros_Catalogo_Lista_Obtener(CodEmpresa, tipo);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasCatalogoData>> CR_PrendasParametros_Catalogo_Lista_Export(int CodEmpresa, string tipo)
        {
            return DB.CR_PrendasParametros_Catalogo_Lista_Export(CodEmpresa, tipo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_PrendasParametros_Catalogos_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_PrendasParametros_Catalogos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto CR_PrendasParametros_Catalogo_Guardar(int CodEmpresa, CrPrendasCatalogoGuardarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Catalogo_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_PrendasParametros_Catalogo_Eliminar(int CodEmpresa, CrPrendasCatalogoEliminarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Catalogo_Eliminar(CodEmpresa, request, usuario);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> CR_PrendasParametros_Coberturas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_PrendasParametros_Coberturas_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasCoberturaData>> CR_PrendasParametros_Coberturas_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_PrendasParametros_Coberturas_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_PrendasParametros_Coberturas_Guardar(int CodEmpresa, CrPrendasCoberturaGuardarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Coberturas_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_PrendasParametros_Coberturas_Eliminar(int CodEmpresa, CrPrendasCoberturaEliminarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Coberturas_Eliminar(CodEmpresa, request, usuario);
        }

        public ErrorDto<List<CrPrendasPolizaF4Data>> CR_PrendasParametros_Polizas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return DB.CR_PrendasParametros_Polizas_F4_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> CR_PrendasParametros_Comercializa_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_PrendasParametros_Comercializa_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasComercializaListaData>> CR_PrendasParametros_Comercializa_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_PrendasParametros_Comercializa_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrPrendasComercializaData> CR_PrendasParametros_Comercializa_Consulta(int CodEmpresa, int codigo)
        {
            return DB.CR_PrendasParametros_Comercializa_Consulta(CodEmpresa, codigo);
        }

        public ErrorDto CR_PrendasParametros_Comercializa_Guardar(int CodEmpresa, CrPrendasComercializaGuardarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Comercializa_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_PrendasParametros_Comercializa_Eliminar(int CodEmpresa, CrPrendasComercializaEliminarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Comercializa_Eliminar(CodEmpresa, request, usuario);
        }

        public ErrorDto<List<CrPrendasComercializaF4Data>> CR_PrendasParametros_Comercializa_F4_Obtener(int CodEmpresa, string? texto)
        {
            return DB.CR_PrendasParametros_Comercializa_F4_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<List<CrPrendasTipoIdData>> CR_PrendasParametros_TiposId_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_PrendasParametros_TiposId_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrPrendasBancoData>> CR_PrendasParametros_Bancos_Dropdown_Obtener(int CodEmpresa)
        {
            return DB.CR_PrendasParametros_Bancos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CrPrendasCuentaData>> CR_PrendasParametros_Cuentas_Lista_Obtener(int CodEmpresa, string identificacion)
        {
            return DB.CR_PrendasParametros_Cuentas_Lista_Obtener(CodEmpresa, identificacion);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> CR_PrendasParametros_Unidades_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return DB.CR_PrendasParametros_Unidades_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrPrendasParametrosLista<CrPrendasUnidadData>> CR_PrendasParametros_Unidades_Lista_Export(int CodEmpresa, string parametros)
        {
            return DB.CR_PrendasParametros_Unidades_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto CR_PrendasParametros_Unidades_Guardar(int CodEmpresa, CrPrendasUnidadGuardarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Unidades_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_PrendasParametros_Unidades_Eliminar(int CodEmpresa, CrPrendasUnidadEliminarRequest request, string usuario)
        {
            return DB.CR_PrendasParametros_Unidades_Eliminar(CodEmpresa, request, usuario);
        }
    }
}