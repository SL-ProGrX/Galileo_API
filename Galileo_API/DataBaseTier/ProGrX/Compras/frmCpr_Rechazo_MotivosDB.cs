using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprRechazoMotivosDB
    {
        private readonly PortalDB _portalDB;

        // Evitar literales repetidos (Sonar)
        private const string DefaultSortField = "COD_RECHAZO";
        private const string SortFieldDescripcion = "DESCRIPCION";
        private const string SortFieldActivo = "ACTIVO";
        private const string ErrorLiteral = "Error";

        private static string Clean(string? value) => (value ?? string.Empty).Trim();

        private static ErrorDto Fail(Exception ex) => DbHelper.ErrorResponse(ex.Message, -1);

        private static int SafeCode(ErrorDto r) => r.Code is int c ? c : -1;

        private static int SafeCode<T>(ErrorDto<T> r) => r.Code is int c ? c : -1;

        private static ErrorDto? ValidateMotivo(CprRechazosMotivosDto? motivo, out string cod)
        {
            cod = string.Empty;
            if (motivo == null)
                return DbHelper.ErrorResponse("Motivo inválido.", -1);

            cod = Clean(motivo.cod_rechazo);
            if (string.IsNullOrWhiteSpace(cod))
                return DbHelper.ErrorResponse("El código del motivo es requerido.", -1);

            return null;
        }

        private ErrorDto<int> GetExistsCount(int codCliente, string cod)
        {
            const string existsSql = @"SELECT COUNT(COD_RECHAZO)
FROM CPR_RECHAZO_TIPOS
WHERE UPPER(COD_RECHAZO) = @cod;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codCliente,
                existsSql,
                0,
                new { cod = cod.ToUpperInvariant() }
            );
        }

        private static ErrorDto MapExec(ErrorDto exec, string okMsg)
        {
            if (exec.Code == 0)
                return DbHelper.OkResponse(okMsg);

            return DbHelper.ErrorResponse(exec.Description ?? ErrorLiteral, SafeCode(exec));
        }

        private ErrorDto InsertMotivo(int codCliente, CprRechazosMotivosDto motivo, string cod)
        {
            const string insertSql = @"INSERT INTO CPR_RECHAZO_TIPOS
(
    COD_RECHAZO,
    DESCRIPCION,
    ACTIVO,
    REGISTRO_FECHA,
    REGISTRO_USUARIO
)
VALUES
(
    @cod_rechazo,
    @descripcion,
    @activo,
    GETDATE(),
    @registro_usuario
);";

            var exec = DbHelper.ExecuteNonQuery(
                _portalDB,
                codCliente,
                insertSql,
                new
                {
                    cod_rechazo = cod,
                    descripcion = Clean(motivo.descripcion),
                    activo = motivo.activo ? 1 : 0,
                    registro_usuario = Clean(motivo.modifica_usuario ?? motivo.registro_usuario)
                }
            );

            return MapExec(exec, "Motivo agregado correctamente");
        }

        private ErrorDto UpdateMotivo(int codCliente, CprRechazosMotivosDto motivo, string cod)
        {
            const string updateSql = @"UPDATE CPR_RECHAZO_TIPOS
   SET DESCRIPCION = @descripcion,
       ACTIVO = @activo,
       MODIFICA_FECHA = GETDATE(),
       MODIFICA_USUARIO = @modifica_usuario
 WHERE COD_RECHAZO = @cod_rechazo;";

            var exec = DbHelper.ExecuteNonQuery(
                _portalDB,
                codCliente,
                updateSql,
                new
                {
                    cod_rechazo = cod,
                    descripcion = Clean(motivo.descripcion),
                    activo = motivo.activo ? 1 : 0,
                    modifica_usuario = Clean(motivo.modifica_usuario)
                }
            );

            return MapExec(exec, "Motivo actualizado correctamente");
        }

        public FrmCprRechazoMotivosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de motivos de rechazo
        /// </summary>
        public ErrorDto<CprRechazosMotivosLista> CprRechazoMotivoLista_Obtener(int CodCliente, string vFiltros)
        {
            FiltrosLazyLoadData filtro;
            try
            {
                filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(vFiltros) ?? new FiltrosLazyLoadData();
            }
            catch
            {
                filtro = new FiltrosLazyLoadData();
            }

            var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro.Trim()}%";

            var off = filtro.pagina < 0 ? 0 : filtro.pagina;
            var take = filtro.paginacion <= 0 ? 10 : filtro.paginacion;

            // Sort whitelist (avoids SQL injection via ORDER BY)
            var sfRaw = string.IsNullOrWhiteSpace(filtro.sortField) ? DefaultSortField : filtro.sortField.Trim();
            var sortField = sfRaw.ToUpperInvariant() switch
            {
                SortFieldDescripcion => SortFieldDescripcion,
                SortFieldActivo => SortFieldActivo,
                DefaultSortField => DefaultSortField,
                _ => DefaultSortField
            };

            // 0=DESC, 1=ASC
            var sortDir = (filtro.sortOrder == 0) ? 0 : 1;

            var empty = new CprRechazosMotivosLista { total = 0, lista = new List<CprRechazosMotivosDto>() };

            const string countSql = @"SELECT COUNT(COD_RECHAZO)
FROM CPR_RECHAZO_TIPOS
WHERE (@q IS NULL OR COD_RECHAZO LIKE @q OR DESCRIPCION LIKE @q);";

            var totalResp = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodCliente, countSql, 0, new { q = like });
            if (totalResp.Code != 0)
            {
                var code = totalResp.Code is int c ? c : -1;
                return DbHelper.CreateErrorResponse<CprRechazosMotivosLista>(totalResp.Description ?? ErrorLiteral, code, empty);
            }

            const string dataSql = @"SELECT COD_RECHAZO, DESCRIPCION, ACTIVO
FROM CPR_RECHAZO_TIPOS
WHERE (@q IS NULL OR COD_RECHAZO LIKE @q OR DESCRIPCION LIKE @q)
ORDER BY
    CASE WHEN @sortField = 'DESCRIPCION' AND @sortDir = 1 THEN DESCRIPCION END ASC,
    CASE WHEN @sortField = 'DESCRIPCION' AND @sortDir = 0 THEN DESCRIPCION END DESC,
    CASE WHEN @sortField = 'ACTIVO' AND @sortDir = 1 THEN ACTIVO END ASC,
    CASE WHEN @sortField = 'ACTIVO' AND @sortDir = 0 THEN ACTIVO END DESC,
    CASE WHEN @sortField = 'COD_RECHAZO' AND @sortDir = 1 THEN COD_RECHAZO END ASC,
    CASE WHEN @sortField = 'COD_RECHAZO' AND @sortDir = 0 THEN COD_RECHAZO END DESC,
    COD_RECHAZO DESC
OFFSET @off ROWS FETCH NEXT @take ROWS ONLY;";

            var listResp = DbHelper.ExecuteListQuery<CprRechazosMotivosDto>(
                _portalDB,
                CodCliente,
                dataSql,
                new { q = like, off, take, sortField, sortDir }
            );

            if (listResp.Code != 0)
            {
                var code = listResp.Code is int c ? c : -1;
                return DbHelper.CreateErrorResponse<CprRechazosMotivosLista>(listResp.Description ?? ErrorLiteral, code, empty);
            }

            var result = new CprRechazosMotivosLista
            {
                total = totalResp.Result,
                lista = listResp.Result ?? new List<CprRechazosMotivosDto>()
            };

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Guarda un motivo de rechazo (insert/update según isNew)
        /// </summary>
        public ErrorDto CprRechazoMotivo_Guardar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            try
            {
                var validation = ValidateMotivo(motivo, out var cod);
                if (validation != null)
                    return validation;

                var existsResp = GetExistsCount(CodCliente, cod);
                if (existsResp.Code != 0)
                    return DbHelper.ErrorResponse(existsResp.Description ?? ErrorLiteral, SafeCode(existsResp));

                var count = existsResp.Result;

                if (motivo.isNew ?? false)
                {
                    if (count > 0)
                        return DbHelper.ErrorResponse($"El motivo de rechazo con el código {cod} ya existe.", -2);

                    return InsertMotivo(CodCliente, motivo, cod);
                }

                if (count == 0)
                    return DbHelper.ErrorResponse($"El motivo de rechazo con el código {cod} no existe.", -3);

                return UpdateMotivo(CodCliente, motivo, cod);
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }

        /// <summary>
        /// Elimina un motivo de rechazo
        /// </summary>
        public ErrorDto cprRechazoMotivo_Eliminar(int CodCliente, string cod_rechazo)
        {
            try
            {
                var cod = Clean(cod_rechazo);
                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("Código de rechazo inválido.", -1);

                const string sql = @"DELETE FROM CPR_RECHAZO_TIPOS WHERE COD_RECHAZO = @cod;";
                var r = DbHelper.ExecuteNonQuery(_portalDB, CodCliente, sql, new { cod });

                if (r.Code == 0)
                    return DbHelper.OkResponse("Motivo eliminado correctamente");

                var code = r.Code is int c ? c : -1;
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, code);
            }
            catch (Exception ex)
            {
                return Fail(ex);
            }
        }
    }
}