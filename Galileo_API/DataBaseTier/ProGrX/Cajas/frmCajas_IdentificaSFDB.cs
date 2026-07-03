using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasIdentificaSfDb
    {
        private readonly PortalDB _portalDB;

        public FrmCajasIdentificaSfDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para identificar casos de depósitos en efectivo no identificados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="nombre"></param>
        /// <param name="usuario"></param>
        /// <param name="pagadorId"></param>
        /// <param name="origenRecursosId"></param>
        /// <param name="casos"></param>
        /// <returns></returns>
        public ErrorDto Cajas_CajasIdentificaSf_Identificar(
            int codEmpresa,
            string cedula,
            string nombre,
            string usuario,
            string pagadorId,
            string origenRecursosId,
            List<TesDepositoIdentificarDto> casos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            ErrorDto? validation = ValidateIdentificacion(cedula, nombre, casos);
            if (validation != null) return validation;

            conn.Open();
            try
            {
                ExecuteIdentificacion(conn, cedula, nombre, usuario, pagadorId, origenRecursosId, casos!);
                return DbHelper.OkResponse("Caso(s) identificado(s) correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        //=============HELPER METHODS================//

        private static ErrorDto? ValidateIdentificacion(
            string cedula,
            string nombre,
            List<TesDepositoIdentificarDto>? casos)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return DbHelper.ErrorResponse("No se ha especificado ningún Id de Cliente válido", -2);

            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.ErrorResponse("No se ha especificado la cédula del cliente.", -2);

            if (casos == null || casos.Count == 0)
                return DbHelper.ErrorResponse("No se ha seleccionado ningún caso!", -2);

            var invalid = casos.FirstOrDefault(IsCasoInvalido);
            if (invalid != null)
                return DbHelper.ErrorResponse("Hay casos con datos inválidos (Banco/Depósito/Documento).", -2);

            return null;
        }

        private static bool IsCasoInvalido(TesDepositoIdentificarDto c) =>
            c.BancoId <= 0 ||
            c.DepositoId <= 0 ||
            string.IsNullOrWhiteSpace(c.Documento);

        private static void ExecuteIdentificacion(
            IDbConnection conn,
            string cedula,
            string nombre,
            string usuario,
            string pagadorId,
            string origenRecursosId,
            List<TesDepositoIdentificarDto> casos)
        {
            using var tx = conn.BeginTransaction();

            foreach (var c in casos)
            {
                conn.Execute(
                    "spCajas_Identifica_TES_Depositos",
                    new
                    {
                        BancoId = c.BancoId,
                        Documento = c.Documento,
                        Cedula = cedula,
                        Nombre = nombre,
                        Usuario = usuario,
                        PagadorId = pagadorId,
                        OrigenRecursosId = origenRecursosId,
                        DepositoId = c.DepositoId
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure
                );
            }

            tx.Commit();
        }

        //=============DB METHODS================//

        /// <summary>
        /// Método para obtener las cuentas bancarias de depósitos en efectivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCajasIdentificaSfDepositoDto>> Cajas_CajasIdentificaSf_Depositos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spCajas_DepositosCuentasBancariasAut 'DP'";
                return conn.Query<FrmCajasIdentificaSfDepositoDto>(query).ToList();
            });
        }

        /// <summary>
        /// Método para obtener las entidades de pago
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajasIdentificaSf_Entidades_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select COD_ENTIDAD_PAGO as 'item', DESCRIPCION AS 'descripcion' 
                                         from SIF_ENTIDADES_PAGO 
                                        WHERE ACTIVA = 1 ORDER BY COD_ENTIDAD_PAGO";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Método para obtener los orígenes de recursos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajasIdentificaSf_Recursos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select COD_ORIGEN_RECURSOS as 'item', DESCRIPCION AS 'descripcion' 
                                          from SIF_ORIGEN_RECURSOS 
                                        WHERE ACTIVA = 1 ORDER BY COD_ORIGEN_RECURSOS";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Método para consultar los trámites de depósitos en efectivo no identificados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="bancoId"></param>
        /// <param name="montoInicio"></param>
        /// <param name="montoHasta"></param>
        /// <param name="numDocumento"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCajasIdentificaSfTramitsRsdto>> Cajas_CajasIdentificaSf_Consultar(
                int codEmpresa,
                DateTime fechaInicio,
                DateTime fechaCorte,
                int bancoId,
                decimal montoInicio,
                decimal montoHasta,
                string? numDocumento)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                        SELECT
                            Tra.DP_TRAMITE_ID      AS dp_tramite_id,
                            Tra.NSolicitud         AS nsolicitud,
                            Tra.Id_Banco           AS id_banco,
                            Bn.Descripcion         AS bancodesc,
                            'DP'                   AS Tipo,
                            Tra.Documento          AS Documento,
                            Tra.Fecha              AS Fecha,
                            Tra.Monto              AS Monto,
                            Tra.Descripcion        AS Descripcion,
                            Tra.Registro_Fecha     AS registro_fecha,
                            Tra.Registro_Usuario   AS registro_usuario 
                        FROM TES_DEPOSITOS_TRAMITE Tra
                        INNER JOIN TES_BANCOS Bn ON Tra.Id_Banco = Bn.Id_Banco
                        WHERE
                            Tra.ID_REQUERIDA = 1
                            AND Tra.IDENTIFICADO = 0
                            AND Tra.Fecha >= @FechaDesde
                            AND Tra.Fecha <= @FechaHasta
                            AND Tra.Id_Banco = @BancoId
                            AND Tra.Monto BETWEEN @MontoInicio AND @MontoHasta
                            AND (@NumDoc IS NULL OR LTRIM(RTRIM(@NumDoc)) = '' OR Tra.Documento LIKE '%' + @NumDoc + '%')
                        ORDER BY Tra.Fecha DESC;";

                var fechaDesde = fechaInicio.Date; // 00:00:00
                var fechaHasta = fechaCorte.Date.AddDays(1).AddTicks(-1); // 23:59:59.9999999

                var data = conn.Query<FrmCajasIdentificaSfTramitsRsdto>(sql, new
                {
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    BancoId = bancoId,
                    MontoInicio = montoInicio,
                    MontoHasta = montoHasta,
                    NumDoc = numDocumento
                }).ToList();

                return data;
            });
        }

    }
}
