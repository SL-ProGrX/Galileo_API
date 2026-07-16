using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioAsgDB
    {
        /// <summary>
        /// Calcula el monto de la ayuda del beneficio según membresía, tope y grupo.
        /// </summary>
        public FxMontosResult fxMonto(int CodCliente, FxMontoModel datos)
        {
            _datosBase = datos;
            var info = new FxMontosResult { Code = 0 };

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);

                var membresia = connection.QueryFirstOrDefault<string>(
                    @"SELECT CASE WHEN estadoactual = 'S' THEN DATEDIFF(d, fechaingreso, GETDATE()) ELSE 0 END AS Membresia
                      FROM socios WHERE cedula = @cedula", new { cedula = datos.cedula });

                if (membresia == null)
                {
                    return new FxMontosResult { Code = 4, Description = "- No se encontró membresía para esta persona en este beneficio" };
                }

                var montoBene = connection.QueryFirstOrDefault<float>(
                    @"SELECT monto FROM afi_beneficio_montos
                      WHERE cod_beneficio = @codBeneficio AND @membresia BETWEEN inicio AND corte",
                    new { codBeneficio = datos.cod_beneficio, membresia });
                info.montoGira = montoBene;

                var montoPagado = ObtenerMontoPagado(connection, datos);

                var mensaje = string.Empty;
                if (montoPagado >= datos.monto && datos.bConsulta == false && !fxValida(CodCliente, ref mensaje).Result && datos.bNuevo == false)
                {
                    return new FxMontosResult { Code = 1, Description = "Ya le fue asignado el monto de la ayuda" };
                }

                if (datos.monto <= 0)
                {
                    return new FxMontosResult { Code = 2, Description = "- No cumple con la membresía para este beneficio" };
                }

                if (datos.bNuevo == false)
                {
                    fxValida(CodCliente, ref mensaje);
                    info.Code = 3;
                    AplicarMontoSegunGrupo(info, datos, montoBene, mensaje);
                }
                else
                {
                    info.monto = 0;
                    info.Description += mensaje;
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Obtiene el monto ya pagado del beneficio (por cédula, y solicitante si aplica).
        /// </summary>
        private static float ObtenerMontoPagado(SqlConnection connection, FxMontoModel datos)
        {
            if (datos.iBeneficiario == 0)
            {
                return connection.QueryFirstOrDefault<float>(
                    "SELECT COALESCE(SUM(monto), 0) AS monto FROM afi_bene_otorga WHERE cod_beneficio = @codBeneficio AND cedula = @cedula",
                    new { codBeneficio = datos.cod_beneficio, cedula = datos.cedula });
            }

            return connection.QueryFirstOrDefault<float>(
                @"SELECT COALESCE(SUM(monto), 0) AS monto FROM afi_bene_otorga
                  WHERE cod_beneficio = @codBeneficio AND cedula = @cedula AND solicita = @solicita",
                new { codBeneficio = datos.cod_beneficio, cedula = datos.cedula, solicita = datos.solicita });
        }

        /// <summary>
        /// Ajusta el monto del resultado según la existencia y disponibilidad del grupo de beneficios.
        /// </summary>
        private static void AplicarMontoSegunGrupo(FxMontosResult info, FxMontoModel datos, float montoBene, string mensaje)
        {
            if (datos.iGrupo > 0)
            {
                if (datos.cMontoRealGrupo >= montoBene && datos.bAsignado == false)
                {
                    info.monto = montoBene;
                    info.Description += mensaje;
                }
                else
                {
                    info.monto = datos.cMontoRealGrupo;
                    info.montoGira = datos.monto;
                    info.disponible = info.monto - info.montoGira;
                    info.Description += mensaje;
                }
            }
            else
            {
                info.monto = datos.monto;
                info.Description += mensaje;
            }
        }

        /// <summary>
        /// Valida el máximo de otorgamiento y las reglas por grupo del beneficio.
        /// </summary>
        public ErrorDto<bool> fxValida(int CodCliente, ref string mensaje)
        {
            var response = new ErrorDto<bool>();
            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);

                var maxOtorga = connection.QueryFirstOrDefault<int>(
                    "SELECT maximo_otorga FROM afi_beneficios WHERE cod_beneficio = @codBeneficio",
                    new { codBeneficio = _datosBase.cod_beneficio });

                _datosBase.iGrupo = connection.QueryFirstOrDefault<int>(
                    "SELECT cod_grupo FROM afi_grupo_beneficio WHERE cod_beneficio = @codBeneficio",
                    new { codBeneficio = _datosBase.cod_beneficio });

                _datosBase.bAsignado = false;

                if (_datosBase.iGrupo > 0)
                {
                    var montoGrupo = connection.QueryFirstOrDefault<float>(
                        "SELECT monto FROM afi_bene_grupos WHERE cod_grupo = @grupo", new { grupo = _datosBase.iGrupo });

                    var dt = connection.QueryFirstOrDefault<AfiBeneMontoData>(
                        @"SELECT COUNT(*) AS cantidad, ISNULL(SUM(B.MONTO), 0) AS monto
                          FROM afi_bene_otorga B
                          INNER JOIN afi_grupo_beneficio G ON B.cod_beneficio = G.cod_beneficio
                          WHERE B.cedula = @cedula", new { cedula = _datosBase.cedula });

                    if (dt != null)
                    {
                        _datosBase.bAsignado = true;
                        if (dt.monto >= montoGrupo)
                        {
                            mensaje += "\n - Sobrepasa el monto asignado al grupo de beneficios ";
                        }
                        else
                        {
                            _datosBase.cMontoRealGrupo = montoGrupo - dt.monto;
                        }
                    }
                }

                if (_datosBase.bConsulta == false)
                {
                    var cantidad = connection.QueryFirstOrDefault<int>(
                        "SELECT ISNULL(COUNT(*), 0) AS cantidad FROM afi_bene_otorga WHERE cod_beneficio = @codBeneficio AND cedula = @cedula",
                        new { codBeneficio = _datosBase.cod_beneficio, cedula = _datosBase.cedula });

                    if (cantidad >= maxOtorga)
                    {
                        mensaje = " - Excede el numero de veces de Otorgamientos del Beneficio";
                    }
                }

                response.Result = mensaje.Length == 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

        /// <summary>
        /// Consulta si el socio tiene membresía activa para el beneficio.
        /// </summary>
        public ErrorDto Menbrecia_Consulta(int CodCliente, string? cedula)
        {
            if (cedula == null)
            {
                return new ErrorDto { Code = 0, Description = string.Empty };
            }

            const string sql = @"SELECT CASE WHEN estadoactual = 'S' THEN DATEDIFF(d, fechaingreso, GETDATE()) ELSE 0 END AS Membresia
                                 FROM socios WHERE cedula = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<string>(sql, new { cedula }));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            var membresia = result.Result;
            return (membresia == null || membresia == "0")
                ? new ErrorDto { Code = -1, Description = "- No se encontro membresia para esta persona en este beneficio" }
                : new ErrorDto { Code = 0, Description = string.Empty };
        }

        /// <summary>
        /// Obtiene el monto del grupo asociado al beneficio.
        /// </summary>
        public ErrorDto Monto_Obtener(int CodCliente, string cod_beneficio, string cedula, string solicita)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var grupo = connection.QueryFirstOrDefault<int>(
                    "SELECT cod_grupo FROM afi_grupo_beneficio WHERE cod_beneficio = @codBeneficio",
                    new { codBeneficio = cod_beneficio });

                var monto = connection.QueryFirstOrDefault<float>(
                    "SELECT monto FROM afi_bene_grupos WHERE cod_grupo = @grupo", new { grupo });

                return monto.ToString();
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? (result.Result ?? "0") : result.Description
            };
        }
    }
}
