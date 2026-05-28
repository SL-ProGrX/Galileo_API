using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhConfiguracionDB
    {
        /// <summary>
        /// Obtiene el catálogo de divisas visible para la configuración de patrimonio.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_Configuracion_Divisas_Obtener(int codEmpresa)
        {
            const string sql = @"
select
    rtrim(COD_DIVISA) as item,
    rtrim(DESCRIPCION) as descripcion
from vSys_Divisas
order by DIVISA_LOCAL desc, COD_DIVISA asc;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene la configuración de cuentas contables de patrimonio y excedentes por divisa.
        /// </summary>
        public ErrorDto<ParametrosPatrimonioDto> AH_Configuracion_Parametros_Obtener(int codEmpresa, string codDivisa)
        {
            var divisa = AH_Configuracion_NormalizarTexto(codDivisa);
            var result = new ParametrosPatrimonioDto();

            if (string.IsNullOrWhiteSpace(divisa))
            {
                return DbHelper.CreateErrorResponse("La divisa es requerida.", -2, result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var data = conn.QueryFirstOrDefault<ParametrosPatrimonioDto>(
                    "spPAT_Parametros",
                    new { Divisa = divisa },
                    commandType: CommandType.StoredProcedure) ?? new ParametrosPatrimonioDto();

                data.cod_divisa = string.IsNullOrWhiteSpace(data.cod_divisa) ? divisa : data.cod_divisa;
                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Valida una cuenta contable y devuelve su máscara y descripción para la pantalla.
        /// </summary>
        public ErrorDto<AhConfiguracionCuentaValidarResponse> AH_Configuracion_Cuenta_Validar(int codEmpresa, string cuenta,int contabilidad)
        {
            var cuentaNormalizada = AH_Configuracion_NormalizarTexto(cuenta);
            var response = new AhConfiguracionCuentaValidarResponse();

            if (string.IsNullOrWhiteSpace(cuentaNormalizada))
            {
                return DbHelper.CreateErrorResponse("La cuenta es requerida.", -2, response);
            }

            try
            {
                var cuentaFormato = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, cuentaNormalizada, 0);
                var cuentaMask = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, true, cuentaFormato, 0);
                var valida = _mCntLinkDb.fxgCntCuentaValida(codEmpresa, cuentaFormato);
                var descripcion = valida ? _mCntLinkDb.fxgCntCuentaDesc(codEmpresa, cuentaFormato, contabilidad) : string.Empty;

                response.cuenta = cuentaFormato;
                response.cuenta_mask = cuentaMask;
                response.cuenta_desc = descripcion;
                response.valida = valida;

                if (!valida)
                {
                    return DbHelper.CreateErrorResponse("No se puede guardar la información, verifique la cuenta ingresada.", -2, response);
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }
    }
}
