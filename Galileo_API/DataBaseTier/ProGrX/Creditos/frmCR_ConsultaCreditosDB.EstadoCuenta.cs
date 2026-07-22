using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public partial class FrmCRConsultaCreditosDB
    {
        #region @

        /// <summary>
        /// Método que obtiene el correo y los periodos de cierre disponibles para un socio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<SocioCierresData> Email_SocioPeriodos_Obtener(int CodEmpresa, string cedula)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                string email = connection.QueryFirstOrDefault<string>(
                    "select rtrim(isnull(AF_Email,'')) as Email from socios where cedula = @cedula",
                    new { cedula }) ?? string.Empty;

                var periodosList = connection.Query<SociosPeriodoData>(
                    "spSys_Periodos_Cierre_Consulta",
                    commandType: CommandType.StoredProcedure).ToList();

                return new SocioCierresData
                {
                    email = email,
                    periodos = periodosList
                        .Skip(1)
                        .Select(p =>
                        {
                            string valor = p?.idx?.Trim() ?? string.Empty;
                            if (DateTime.TryParse(valor, out DateTime fechaCorte))
                            {
                                valor = fechaCorte.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                            }

                            return new DropDownListaGenericaModel
                            {
                                item = valor,
                                descripcion = p?.itmx?.Trim() ?? string.Empty
                            };
                        })
                        .ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new SocioCierresData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar correo y periodos del socio.", result.Code.GetValueOrDefault(-1), new SocioCierresData());
        }

        public ErrorDto Email_SocioEstadoCuenta_Enviar(int CodEmpresa, string usuario, string cedula, string email, string periodo, string tipo)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.ErrorResponse("La identificación de la persona es requerida.", -1);

            if (string.IsNullOrWhiteSpace(email))
                return DbHelper.ErrorResponse("La persona no cuenta con un correo registrado.", -1);

            if (tipo != "T" && tipo != "C")
                return DbHelper.ErrorResponse("El tipo de estado de cuenta no es válido.", -1);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                if (tipo == "T")
                {
                    connection.Query(
                        "spuProGrX_MOBILE_CUENTAS_ENVIAESTADO",
                        new { cedula },
                        commandType: CommandType.StoredProcedure);

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Estado de Cuenta: [email] {cedula.Trim()}",
                        Movimiento = "Aplica - WEB",
                        Modulo = 10
                    });

                    return DbHelper.OkResponse("Estado de Cuenta enviado al Correo Electrónico registrado de la persona!");
                }

                if (!DateTime.TryParseExact(
                    periodo,
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime vCorte))
                {
                    return DbHelper.ErrorResponse("Seleccione un período de corte válido.", -1);
                }

                return _mProGrx_Main.sbEstadoCuenta_Email_Corte(CodEmpresa, usuario, cedula, email, vCorte);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al enviar estado de cuenta.", result.Code.GetValueOrDefault(-1));
        }



        #endregion
    }
}

