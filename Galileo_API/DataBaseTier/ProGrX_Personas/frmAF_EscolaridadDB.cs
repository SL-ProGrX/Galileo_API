using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using System;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfEscolaridadDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 1;
        private readonly string EscolaridadTipoField = "Escolaridad_Tipo";

        public FrmAfEscolaridadDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de tipos de escolaridad según filtros y ordenamiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de paginación, búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de tipos de escolaridad</returns>
        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            var sortMap = new Dictionary<string, int>
            {
                [EscolaridadTipoField] = 1,
                ["descripcion"] = 2,
                ["ACTIVO"] = 3,
                ["Registro_Fecha"] = 4,
                ["Registro_Usuario"] = 5
            };
            var spec = Galileo.DataBaseTier.LazyLoadHelper.Build(filtros, sortMap, EscolaridadTipoField);
            string where = spec.HasFilter
                ? $"WHERE ({EscolaridadTipoField} LIKE @filtro OR descripcion LIKE @filtro OR Registro_Usuario LIKE @filtro)"
                : "";
            string sortField = string.IsNullOrWhiteSpace(filtros?.sortField) ? EscolaridadTipoField : filtros.sortField;
            string queryTotal = $"SELECT COUNT({EscolaridadTipoField}) FROM AFI_ESCOLARIDAD_TIPOS {where}";
            string queryLista = $@"SELECT {EscolaridadTipoField}, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               FROM AFI_ESCOLARIDAD_TIPOS
                               {where}
                               ORDER BY {sortField} {(spec.IsAsc ? "ASC" : "DESC")}
                               OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryTotal, 0, spec.Params);
            var lista = DbHelper.ExecuteListQuery<NivelEscolaridadData>(_portalDb, codEmpresa, queryLista, spec.Params);
            return new ErrorDto<NivelEscolaridadLista>
            {
                Code = 0,
                Description = "OK",
                Result = new NivelEscolaridadLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de escolaridad según si existe o no.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="escolaridad">Datos del tipo de escolaridad</param>
        /// <returns>ErrorDto con el resultado de la operación</returns>
        public ErrorDto AF_EscolaridadTipos_Guardar(int codEmpresa, string usuario, NivelEscolaridadData escolaridad)
        {
            string queryExiste = $"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESCOLARIDAD_TIPOS WHERE {EscolaridadTipoField} = @{EscolaridadTipoField}";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { escolaridad.Escolaridad_Tipo });
            return existe.Result == 0
                ? AF_EscolaridadTipos_Insertar(codEmpresa, usuario, escolaridad)
                : AF_EscolaridadTipos_Actualizar(codEmpresa, usuario, escolaridad);
        }

        private ErrorDto AF_EscolaridadTipos_Insertar(int codEmpresa, string usuario, NivelEscolaridadData escolaridad)
        {
            string query = $@"INSERT INTO AFI_ESCOLARIDAD_TIPOS ({EscolaridadTipoField}, Descripcion, ACTIVO, registro_fecha, registro_usuario)
                              VALUES (@Escolaridad_Tipo, @Descripcion, @ACTIVO, GETDATE(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                escolaridad.Escolaridad_Tipo,
                escolaridad.Descripcion,
                ACTIVO = escolaridad.Activo ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Tipo de Escolaridad : {escolaridad.Escolaridad_Tipo}");
            }
            return result;
        }

        private ErrorDto AF_EscolaridadTipos_Actualizar(int codEmpresa, string usuario, NivelEscolaridadData escolaridad)
        {
            string query = $@"UPDATE AFI_ESCOLARIDAD_TIPOS
                              SET Descripcion = @Descripcion,
                                  ACTIVO = @ACTIVO
                              WHERE {EscolaridadTipoField} = @Escolaridad_Tipo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                escolaridad.Escolaridad_Tipo,
                escolaridad.Descripcion,
                ACTIVO = escolaridad.Activo ? 1 : 0
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Tipo de Escolaridad : {escolaridad.Escolaridad_Tipo}");
            }
            return result;
        }

        /// <summary>
        /// Elimina un tipo de escolaridad por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="escolaridadTipo">Identificador del tipo de escolaridad</param>
        /// <returns>ErrorDto con el resultado de la eliminación</returns>
        public ErrorDto AF_EscolaridadTipos_Eliminar(int codEmpresa, string usuario, string escolaridadTipo)
        {
            string query = $"DELETE FROM AFI_ESCOLARIDAD_TIPOS WHERE {EscolaridadTipoField} = @Escolaridad_Tipo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { Escolaridad_Tipo = escolaridadTipo });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Tipo de Escolaridad : {escolaridadTipo}");
            }
            return result;
        }

        /// <summary>
        /// Valida si un tipo de escolaridad ya existe por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="escolaridadTipo">Identificador del tipo de escolaridad</param>
        /// <returns>ErrorDto indicando si el tipo de escolaridad existe o es válido</returns>
        public ErrorDto AF_EscolaridadTipos_Valida(int codEmpresa, string escolaridadTipo)
        {
            string query = $"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESCOLARIDAD_TIPOS WHERE {EscolaridadTipoField} = @Escolaridad_Tipo";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { Escolaridad_Tipo = escolaridadTipo }).Result;
            return existe > 0
                ? new ErrorDto { Code = -1, Description = "El tipo de escolaridad ya existe." }
                : new ErrorDto { Code = 0, Description = "El tipo de escolaridad es válido." };
        }

        /// <summary>
        /// Exporta la lista de tipos de escolaridad según filtros (sin paginación).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda y ordenamiento</param>
        /// <returns>ErrorDto con la lista de tipos de escolaridad</returns>
        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Exportar(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            var sortMap = new Dictionary<string, int>
            {
                ["ESCOLARIDAD_TIPO"] = 1,
                ["descripcion"] = 2,
                ["ACTIVO"] = 3,
                ["Registro_Fecha"] = 4,
                ["Registro_Usuario"] = 5
            };
            var spec = Galileo.DataBaseTier.LazyLoadHelper.Build(filtros, sortMap, "ESCOLARIDAD_TIPO");
            string where = spec.HasFilter
                ? "WHERE (ESCOLARIDAD_TIPO LIKE @filtro OR descripcion LIKE @filtro OR Registro_Usuario LIKE @filtro)"
                : "";
            string sortField = string.IsNullOrWhiteSpace(filtros?.sortField) ? "ESCOLARIDAD_TIPO" : filtros.sortField;
            string query = $@"SELECT {EscolaridadTipoField}, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               FROM AFI_ESCOLARIDAD_TIPOS
                               {where}
                               ORDER BY {sortField} {(spec.IsAsc ? "ASC" : "DESC")}";
            var lista = DbHelper.ExecuteListQuery<NivelEscolaridadData>(_portalDb, codEmpresa, query, spec.Params);
            return new ErrorDto<NivelEscolaridadLista>
            {
                Code = 0,
                Description = "OK",
                Result = new NivelEscolaridadLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }
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
