using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHExcedentesAplicadoresDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 2;

        public FrmAHExcedentesAplicadoresDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta un movimiento en la bitácora del sistema.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista completa de usuarios aplicadores de excedentes.
        /// La búsqueda, ordenamiento y exportación se manejan en frontend.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ExcedenteAplicadorDto>>
            AH_Excedentes_Aplicadores_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    SELECT
                        RTRIM(A.USUARIO) AS usuario,
                        ISNULL(A.ACTIVO, 0) AS activo,
                        ISNULL(A.CARGA, 0) AS carga,
                        ISNULL(A.REAL, 0) AS real,
                        ISNULL(A.PROYECTADO, 0) AS proyectado,
                        ISNULL(A.PRORRATEADO, 0) AS prorrateado
                    FROM EXC_APLICADORES A
                    LEFT JOIN USUARIOS U
                        ON A.USUARIO = U.NOMBRE
                    WHERE U.ESTADO = 'A'
                    ORDER BY
                        A.ACTIVO DESC,
                        A.USUARIO;";

                return conn.Query<ExcedenteAplicadorDto>(sql).ToList();
            });
        }

        /// <summary>
        /// Registra un nuevo aplicador o actualiza sus indicadores.
        /// El usuario no se modifica una vez creado.
        /// Replica el comportamiento de spExc_Aplicadores_Add utilizado por VB6.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="aplicador"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Aplicadores_Guardar(
            int CodEmpresa,
            string usuario,
            ExcedenteAplicadorDto aplicador)
        {
            usuario = (usuario ?? string.Empty).Trim();
            aplicador.usuario = (aplicador.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(aplicador.usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario aplicador."
                };
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario del sistema."
                };
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Execute(
                    "spExc_Aplicadores_Add",
                    new
                    {
                        A_Usuario = aplicador.usuario,
                        Mov = "A",
                        Usuario = usuario,
                        Activo = aplicador.activo,
                        Carga = aplicador.carga,
                        Real = aplicador.real,
                        Proyectado = aplicador.proyectado,
                        Prorrateado = aplicador.prorrateado
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento =
                        $"Excedentes> Usuario Aplicador: {aplicador.usuario}",
                    Movimiento = "REGISTRA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(
                    "Aplicador guardado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un usuario aplicador de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioAplicador"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Aplicadores_Eliminar(
            int CodEmpresa,
            string usuarioAplicador,
            string usuario)
        {
            usuarioAplicador = (usuarioAplicador ?? string.Empty).Trim();
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuarioAplicador))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario aplicador."
                };
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario del sistema."
                };
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    DELETE FROM EXC_APLICADORES
                    WHERE USUARIO = @usuarioAplicador;";

                int rows = conn.Execute(
                    sql,
                    new
                    {
                        usuarioAplicador
                    });

                if (rows <= 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description =
                            $"No se encontró el usuario aplicador {usuarioAplicador}."
                    };
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento =
                        $"Excedentes: Usuario Aplicador: {usuarioAplicador}",
                    Movimiento = "ELIMINA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(
                    "Aplicador eliminado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}