using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo.DataBaseTier;
using Galileo.Models;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfMotivosIngresoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmAfMotivosIngresoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de motivos de ingreso según filtros y ordenamiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de paginación, búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de motivos de ingreso</returns>
        public ErrorDto<MotivoIngresoLista> AF_MotivosIngreso_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            object? parametros = null;
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                where = " WHERE (Cod_Motivo LIKE @Filtro OR Descripcion LIKE @Filtro OR Registro_Usuario LIKE @Filtro)";
                parametros = new { Filtro = "%" + filtros.filtro + "%" };
            }

            string order = filtros?.sortField switch
            {
                "Cod_Motivo" => "Cod_Motivo",
                "Descripcion" => "Descripcion",
                "Activo" => "Activo",
                "Registro_Fecha" => "Registro_Fecha",
                "Registro_Usuario" => "Registro_Usuario",
                _ => "Cod_Motivo"
            };
            string sortOrder = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

            int pagina = filtros?.pagina ?? 0;
            int paginacion = filtros?.paginacion ?? 10;
            int rowStart = (pagina * paginacion) + 1;
            int rowEnd = rowStart + paginacion - 1;

            string sqlTotal = $"SELECT COUNT(Cod_Motivo) FROM AFI_MOTIVOS_INGRESOS {where}";
            string sqlLista = $@"
                SELECT Cod_Motivo, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                FROM (
                    SELECT Cod_Motivo, Descripcion, Activo, Registro_Fecha, Registro_Usuario,
                           ROW_NUMBER() OVER (ORDER BY {order} {sortOrder}) AS RowNum
                    FROM AFI_MOTIVOS_INGRESOS
                    {where}
                ) AS T
                WHERE RowNum >= @RowStart AND RowNum <= @RowEnd
                ORDER BY RowNum";

            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlTotal, 0, parametros);
            var lista = DbHelper.ExecuteListQuery<MotivoIngresoData>(_portalDb, codEmpresa, sqlLista, new
            {
                Filtro = parametros is null ? null : ((dynamic)parametros).Filtro,
                RowStart = rowStart,
                RowEnd = rowEnd
            });

            return new ErrorDto<MotivoIngresoLista>
            {
                Code = 0,
                Description = "OK",
                Result = new MotivoIngresoLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Valida si un motivo de ingreso ya existe por su código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="codMotivo">Código del motivo de ingreso</param>
        /// <returns>ErrorDto indicando si el motivo existe o es válido</returns>
        public ErrorDto AF_MotivosIngreso_Valida(int codEmpresa, string codMotivo)
        {
            var result = new ErrorDto();
            string query = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_MOTIVOS_INGRESOS WHERE Cod_Motivo = @Cod_Motivo";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { Cod_Motivo = codMotivo }).Result;
            if (existe > 0)
            {
                result.Code = -1;
                result.Description = "El motivo de ingreso ya existe.";
            }
            else
            {
                result.Code = 0;
                result.Description = "El motivo de ingreso es válido.";
            }
            return result;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un motivo de ingreso según si existe o no.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="motivoIngreso">Datos del motivo de ingreso</param>
        /// <returns>ErrorDto con el resultado de la operación</returns>
        public ErrorDto AF_MotivosIngreso_Guardar(int codEmpresa, string usuario, MotivoIngresoData motivoIngreso)
        {
            // Validar existencia
            string queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_MOTIVOS_INGRESOS WHERE Cod_Motivo = @Cod_Motivo";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { motivoIngreso.Cod_Motivo });

            ErrorDto result;
            if (existe.Result == 0)
            {
                // Insertar
                result = AF_MotivosIngreso_Insertar(codEmpresa, usuario, motivoIngreso);
            }
            else
            {
                // Actualizar
                result = AF_MotivosIngreso_Actualizar(codEmpresa, usuario, motivoIngreso);
            }
            return result;
        }
        /// <summary>
        /// Inserta un nuevo motivo de ingreso en la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="motivoIngreso">Datos del motivo de ingreso</param>
        /// <returns>ErrorDto con el resultado de la inserción</returns>
        private ErrorDto AF_MotivosIngreso_Insertar(int codEmpresa, string usuario, MotivoIngresoData motivoIngreso)
        {
            string query = @"INSERT INTO AFI_MOTIVOS_INGRESOS (Cod_Motivo, Descripcion, Activo, registro_fecha, registro_usuario)
                     VALUES (@Cod_Motivo, @Descripcion, @Activo, dbo.myGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new
            {
                motivoIngreso.Cod_Motivo,
                motivoIngreso.Descripcion,
                Activo = motivoIngreso.Activo ? 1 : 0,
                Usuario = usuario
            });

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Motivo de Ingreso : {motivoIngreso.Cod_Motivo} - {motivoIngreso.Descripcion}");
            }
            return result;
        }

        /// <summary>
        /// Actualiza un motivo de ingreso existente en la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="motivoIngreso">Datos del motivo de ingreso</param>
        /// <returns>ErrorDto con el resultado de la actualización</returns>
        private ErrorDto AF_MotivosIngreso_Actualizar(int codEmpresa, string usuario, MotivoIngresoData motivoIngreso)
        {
            string query = @"UPDATE AFI_MOTIVOS_INGRESOS
                     SET Descripcion = @Descripcion,
                         Activo = @Activo
                     WHERE Cod_Motivo = @Cod_Motivo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new
            {
                motivoIngreso.Cod_Motivo,
                motivoIngreso.Descripcion,
                Activo = motivoIngreso.Activo ? 1 : 0
            });

            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Motivo de Ingreso : {motivoIngreso.Cod_Motivo} - {motivoIngreso.Descripcion}");
            }
            return result;
        }

        /// <summary>
        /// Elimina un motivo de ingreso por su código.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="codMotivo">Código del motivo de ingreso</param>
        /// <returns>ErrorDto con el resultado de la eliminación</returns>
        public ErrorDto AF_MotivosIngreso_Eliminar(int codEmpresa, string usuario, string codMotivo)
        {
            string query = "DELETE FROM AFI_MOTIVOS_INGRESOS WHERE Cod_Motivo = @Cod_Motivo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { Cod_Motivo = codMotivo });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Motivo de Ingreso : {codMotivo}");
            }
            return result;
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
