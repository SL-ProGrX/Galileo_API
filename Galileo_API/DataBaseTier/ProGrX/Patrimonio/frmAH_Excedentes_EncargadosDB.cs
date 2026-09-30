using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHExcedentesEncargadosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 2;

        public FrmAHExcedentesEncargadosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta un movimiento en la bitácora.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista completa de encargados del proceso de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<EncargadoExcedenteDto>>
            AH_Excedentes_Encargados_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                return conn.Query<EncargadoExcedenteDto>(
                    "spExc_Encargados",
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Registra o actualiza un encargado del proceso de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="encargado"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Encargados_Guardar(
            int CodEmpresa,
            string usuario,
            EncargadoExcedenteDto encargado)
        {
            usuario = (usuario ?? string.Empty).Trim();
            encargado.usuario = (encargado.usuario ?? string.Empty).Trim();
            encargado.email = (encargado.email ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(encargado.usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario encargado."
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
                    "spExc_Encargados_Add",
                    new
                    {
                        Usuario = encargado.usuario,
                        Mov = "A",
                        A_Usuario = usuario,
                        Email = encargado.email,
                        Activo = encargado.activo == true ? 1 : 0
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento =
                        $"Usuario Encargo de Excedentes: {encargado.usuario}",
                    Movimiento = "REGISTRA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(
                    "Encargado guardado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un encargado del proceso de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioEncargado"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Encargados_Eliminar(
            int CodEmpresa,
            string usuarioEncargado,
            string usuario)
        {
            usuarioEncargado = (usuarioEncargado ?? string.Empty).Trim();
            usuario = (usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuarioEncargado))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario encargado."
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
                    "spExc_Encargados_Add",
                    new
                    {
                        Usuario = usuarioEncargado,
                        Mov = "B",
                        A_Usuario = usuario,
                        Email = string.Empty,
                        Activo = 0
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento =
                        $"Usuario Encargo de Excedentes: {usuarioEncargado}",
                    Movimiento = "ELIMINA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(
                    "Encargado eliminado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los usuarios activos disponibles para seleccionar como encargados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            AH_Excedentes_Encargados_Usuarios_Dropdown_Obtener(
                int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    SELECT
                        RTRIM(NOMBRE) AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM USUARIOS
                    WHERE ESTADO = 'A'
                    ORDER BY NOMBRE;";

                return conn
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();
            });
        }
    }
}