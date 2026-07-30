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
        #region Aut/C.I

        /// <summary>
        /// Registra el consentimiento informado de la persona y sus bitácoras.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_RegistraConsentimiento(int codEmpresa, string cedula, string usuario)
        {
            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var usuarioNormalizado = (usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
                return DbHelper.ErrorResponse("La identificación de la persona es requerida.", -1);

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
                return DbHelper.ErrorResponse("El usuario que aprueba el consentimiento es requerido.", -1);

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var persona = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                    @"SELECT Consentimiento_Contacto_Fecha,
                             RTRIM(Nombre) AS Nombre
                      FROM SOCIOS
                      WHERE CEDULA = @Cedula",
                    new { Cedula = cedulaNormalizada });

                if (persona is null)
                    throw new InvalidOperationException("No se encontró la persona indicada.");

                if (persona.consentimiento_contacto_fecha.HasValue)
                    throw new InvalidOperationException("El consentimiento de uso de información ya fue aprobado anteriormente!");

                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", cedulaNormalizada);
                parameters.Add("@Indicador", 29);
                parameters.Add("@Valor", 1);
                parameters.Add("@Usuario", usuarioNormalizado);

                connection.Execute("spAFI_Persona_Indicadores", parameters, commandType: CommandType.StoredProcedure);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuarioNormalizado,
                    DetalleMovimiento = $"Firma Consentimiento Informado a Ced.{cedulaNormalizada}",
                    Movimiento = "Aplica - WEB",
                    Modulo = 10
                });

                _mAfilicacionDB.sbgAFIBitacora_Registrar(
                    codEmpresa,
                    "29",
                    $"Id.: {cedulaNormalizada} - {persona.nombre?.Trim()}",
                    cedulaNormalizada,
                    usuarioNormalizado);

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar consentimiento.", result.Code.GetValueOrDefault(-1));
        }


        #endregion
    }
}

