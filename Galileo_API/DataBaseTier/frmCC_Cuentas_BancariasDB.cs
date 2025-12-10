using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCcCuentasBancariasDb
    {
        private readonly PortalDB _portalDB;

        public FrmCcCuentasBancariasDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        #region CC

        /// <summary>
        /// Obtiene la lista de Bancos 
        /// </summary>
        public List<BancosCC> BancosCC_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(Tg.COD_GRUPO) AS COD_GRUPO,
                       RTRIM(Tg.DESCRIPCION) AS DESCRIPCION
                FROM Tes_Bancos B 
                INNER JOIN tes_banco_docs D 
                    ON B.id_banco = D.id_banco 
                   AND D.tipo = 'TE' 
                INNER JOIN TES_BANCOS_GRUPOS Tg 
                    ON B.cod_Grupo = Tg.cod_grupo 
                WHERE Tg.Activo = 1 
                GROUP BY Tg.COD_GRUPO, Tg.DESCRIPCION 
                ORDER BY Tg.COD_GRUPO;";

            var result = DbHelper.ExecuteListQuery<BancosCC>(_portalDB, CodEmpresa, sql);
            return result.Result ?? new List<BancosCC>();
        }

        /// <summary>
        /// Datos para validar la cuenta
        /// </summary>
        public ValidacionCC ValidacionCC_Obtener(int CodEmpresa, string Cod_Grupo)
        {
            const string sql = @"
                SELECT LCTA_Interna, LCTA_InterBancaria
                FROM tes_Bancos_Grupos  
                WHERE cod_grupo = @CodGrupo;";

            var result = DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                defaultValue: new ValidacionCC(),
                parameters: new { CodGrupo = Cod_Grupo });

            return result.Result ?? new ValidacionCC();
        }

        /// <summary>
        /// Valida si ya existe la cuenta para la cédula
        /// </summary>
        public int ValidaCC_Existe(int CodEmpresa, string cuenta, string cedula)
        {
            const string sql = @"
                SELECT COUNT(*) 
                FROM SYS_CUENTAS_BANCARIAS 
                WHERE Identificacion = @Cedula
                  AND CUENTA_INTERNA = @Cuenta;";

            var result = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                CodEmpresa,
                sql,
                defaultValue: 0,
                parameters: new
                {
                    Cedula = cedula,
                    Cuenta = cuenta
                });

            return result.Result;
        }

        /// <summary>
        /// Obtiene la lista de cuentas bancarias de la cédula
        /// </summary>
        public List<SysCuentasBancariasDto> CuentasBancarias_Obtener(int CodEmpresa, string cedula, string? modulo)
        {
            const string baseSql = @"
                SELECT 
                    RTRIM(C.cod_Banco) AS cod_banco,
                    RTRIM(B.Descripcion) AS DESCRIPCION,
                    CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS Tipo_Desc,
                    C.tipo,
                    C.cod_Divisa,
                    C.CUENTA_INTERNA,
                    C.DESTINO,
                    C.CUENTA_INTERBANCA,
                    C.ACTIVA,
                    C.REGISTRO_FECHA,
                    C.REGISTRO_USUARIO,
                    C.identificacion,
                    C.Modulo,
                    ISNULL(C.CUENTA_DEFAULT,0) AS CUENTA_DEFAULT,
                    (C.identificacion + '-' + C.CUENTA_INTERNA + '-' + C.cod_banco) AS DataKey
                FROM SYS_CUENTAS_BANCARIAS C 
                INNER JOIN TES_BANCOS_GRUPOS B 
                    ON C.cod_banco = B.cod_grupo
                WHERE C.Identificacion = @Cedula";

            var parameters = new DynamicParameters();
            parameters.Add("@Cedula", cedula);

            string sql = baseSql;

            if (!string.IsNullOrEmpty(modulo) && modulo != "null")
            {
                sql += " AND C.Modulo = @Modulo";
                parameters.Add("@Modulo", modulo);
            }

            var result = DbHelper.ExecuteListQuery<SysCuentasBancariasDto>(
                _portalDB,
                CodEmpresa,
                sql,
                parameters);

            return result.Result ?? new List<SysCuentasBancariasDto>();
        }

        /// <summary>
        /// Actualiza la Cuenta Bancaria 
        /// </summary>
        public ErrorDto CuentaBancaria_Actualizar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            const string sql = @"
                UPDATE SYS_CUENTAS_BANCARIAS SET 
                    cod_banco        = @Cod_Banco,
                    Tipo             = @Tipo,
                    cod_Divisa       = @Cod_Divisa,
                    DESTINO          = @Destino,
                    CUENTA_INTERNA   = @Cuenta_Interna,
                    CUENTA_INTERBANCA= @Cuenta_Interbanca,
                    CUENTA_DEFAULT   = @Cuenta_Default,
                    ACTIVA           = @Activa,
                    Modulo           = @Modulo,
                    registro_usuario = @Registro_Usuario,
                    registro_fecha   = @Registro_Fecha
                WHERE CUENTA_INTERNA = @Cuenta_Interna
                  AND Identificacion = @Identificacion;";

            // Normalizo flags numéricos igual que antes (Convert.ToInt32)
            var parameters = new
            {
                data.Cod_Banco,
                data.Tipo,
                data.Cod_Divisa,
                data.Destino,
                data.Cuenta_Interna,
                Cuenta_Interbanca = Convert.ToInt32(data.Cuenta_Interbanca),
                Cuenta_Default    = Convert.ToInt32(data.Cuenta_Default),
                Activa            = Convert.ToInt32(data.Activa),
                data.Modulo,
                data.Registro_Usuario,
                data.Registro_Fecha,
                data.Identificacion
            };

            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var rows = connection.Execute(sql, parameters);
                resp.Code = rows;
                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Crea la cuenta bancaria
        /// </summary>
        public ErrorDto CuentaBancaria_Insertar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            const string sql = @"
                INSERT INTO SYS_CUENTAS_BANCARIAS(
                    Identificacion,
                    cod_banco,
                    tipo,
                    cod_divisa,
                    modulo,
                    DESTINO,
                    CUENTA_INTERNA,
                    CUENTA_INTERBANCA,
                    CUENTA_DEFAULT,
                    ACTIVA,
                    REGISTRO_USUARIO,
                    REGISTRO_FECHA
                )
                VALUES (
                    @Identificacion,
                    @Cod_Banco,
                    @Tipo,
                    @Cod_Divisa,
                    @Modulo,
                    @Destino,
                    @Cuenta_Interna,
                    @Cuenta_Interbanca,
                    @Cuenta_Default,
                    @Activa,
                    @Registro_Usuario,
                    GETDATE()
                );";

            var parameters = new
            {
                data.Identificacion,
                data.Cod_Banco,
                data.Tipo,
                data.Cod_Divisa,
                data.Modulo,
                data.Destino,
                data.Cuenta_Interna,
                Cuenta_Interbanca = Convert.ToInt32(data.Cuenta_Interbanca),
                Cuenta_Default    = Convert.ToInt32(data.Cuenta_Default),
                Activa            = Convert.ToInt32(data.Activa),
                data.Registro_Usuario
            };

            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var rows = connection.Execute(sql, parameters);
                resp.Code = rows;
                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Borra la cuenta del cliente
        /// </summary>
        public ErrorDto CuentaBancaria_Borrar(int CodEmpresa, SysCuentasBancariasDto data)
        {
            var resp = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // valido si la cuenta ya está en una transacción
                const string sqlValida = @"
                    SELECT COUNT(1) 
                    FROM Tes_Transacciones 
                    WHERE CTA_AHORROS = @Cuenta;";

                var cuentaNormalizada = data.Cuenta_Interna.Replace("-", "").Trim();

                var existe = connection.QueryFirstOrDefault<int>(
                    sqlValida,
                    new { Cuenta = cuentaNormalizada });

                if (existe > 0)
                {
                    resp.Code = -1;
                    resp.Description = "No se puede eliminar la cuenta, ya que está asociada a una transacción.";
                    return resp;
                }

                const string sqlDelete = @"
                    DELETE FROM SYS_CUENTAS_BANCARIAS 
                    WHERE Identificacion = @Identificacion
                      AND cod_Banco = @Cod_Banco
                      AND cuenta_Interna = @Cuenta_Interna;";

                var rows = connection.Execute(sqlDelete, new
                {
                    Identificacion = data.Identificacion.Trim(),
                    Cod_Banco      = data.Cod_Banco.Trim(),
                    Cuenta_Interna = data.Cuenta_Interna.Trim()
                });

                resp.Code = rows;
                resp.Description = "Ok";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        #endregion
    }
}