using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public partial class FrmTesBancosCargadoDB
    {
        public ErrorDto<List<TesBancosCargadoChequeCambiadoDto>>
            TES_BancosCargado_ChequesCambiados_Obtener(
                int CodEmpresa,
                string usuario,
                TesBancosCargadoChequesCambiadosObtenerRequest request)
        {
            var errores = ValidarChequesCarga(request, usuario);
            if (errores.Length > 0)
            {
                return DbHelper.CreateErrorResponse<List<TesBancosCargadoChequeCambiadoDto>>(
                    errores.ToString());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();

            using var transaction = conn.BeginTransaction();
            var idCarga = CrearCargaMovimientosBancarios(conn, transaction, usuario.Trim());

            try
            {
                foreach (var cheque in request.Cheques)
                {
                    conn.Execute(
                        @"
                        INSERT INTO dbo.TesCargaMovimientosBancariosDetalleTemp
                            (IdCarga, FechaTransaccion, Documento, NumeroCheque, Monto, IdBanco, TipoMovimiento, RegistroUsuario)
                        VALUES
                            (@IdCarga, @FechaTransaccion, @Documento, @NumeroCheque, @Monto, @IdBanco, @TipoMovimiento, @RegistroUsuario);",
                        new
                        {
                            IdCarga = idCarga,
                            FechaTransaccion = cheque.Fecha!.Value.Date,
                            Documento = cheque.Documento?.Trim(),
                            NumeroCheque = cheque.NumeroCheque?.Trim(),
                            Monto = cheque.Monto!.Value,
                            IdBanco = ObtenerIdBanco(cheque),
                            TipoMovimiento = string.IsNullOrWhiteSpace(cheque.TipoMovimiento)
                                ? "D"
                                : cheque.TipoMovimiento.Trim().ToUpperInvariant(),
                            RegistroUsuario = usuario.Trim()
                        },
                        transaction);
                }

                var result = conn.Query<TesBancosCargadoChequeCambiadoDto>(
                    "dbo.spTesObtenerInformacionCheques",
                    new { pConsecutivoCarga = idCarga },
                    transaction,
                    commandType: CommandType.StoredProcedure)
                    .ToList();

                conn.Execute(
                    "DELETE dbo.TesCargaMovimientosBancariosDetalleTemp WHERE IdCarga = @IdCarga;",
                    new { IdCarga = idCarga },
                    transaction);

                transaction.Commit();
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return DbHelper.CreateErrorResponse<List<TesBancosCargadoChequeCambiadoDto>>(
                    ex.Message);
            }
        }

        public ErrorDto TES_BancosCargado_ChequesCambiados_Procesar(
            int CodEmpresa,
            string usuario,
            TesBancosCargadoChequesCambiadosProcesarRequest request)
        {
            var errores = ValidarChequesProceso(request, usuario);
            if (errores.Length > 0)
            {
                return DbHelper.ErrorResponse(errores.ToString());
            }

            var procesables = request.Cheques
                .Where(EsChequeProcesable)
                .ToList();

            if (procesables.Count == 0)
            {
                return DbHelper.ErrorResponse(
                    "No existen cheques en estado NO ASOCIADO para procesar.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();

            var mensajes = new StringBuilder();
            var procesados = 0;

            try
            {
                foreach (var cheque in procesables)
                {
                    var resultado = conn.Query<TesBancosCargadoChequeProcesoResultadoDto>(
                        "dbo.spTesAsociarDocumentosCheques",
                        new
                        {
                            pDocumentoBanco = cheque.Documento?.Trim(),
                            pNsolicitud = cheque.Solicitud,
                            pUsuario = usuario.Trim(),
                            pCheque = cheque.NumeroCheque?.Trim(),
                            pFechaTransaccion = cheque.FechaTransaccion!.Value.Date,
                            pMontoDebito = cheque.Monto,
                            pIdBanco = cheque.Id_Banco
                        },
                        commandType: CommandType.StoredProcedure)
                        .ToList();

                    if (resultado.Count == 0)
                    {
                        procesados++;
                        continue;
                    }

                    foreach (var descripcion in resultado
                        .Where(item => !string.IsNullOrWhiteSpace(item.descripcion)))
                    {
                        mensajes.AppendLine(descripcion.descripcion);
                    }
                }

                if (mensajes.Length > 0)
                {
                    return DbHelper.ErrorResponse(mensajes.ToString());
                }

                return DbHelper.OkResponse(
                    $"Proceso realizado satisfactoriamente. Cheques asociados: {procesados}.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static int CrearCargaMovimientosBancarios(
            IDbConnection conn,
            IDbTransaction transaction,
            string usuario)
        {
            return conn.Query<int>(
                @"
                INSERT INTO dbo.TesCargaMovimientosBancariosTemp
                    (Descripcion, RegistroFecha, RegistroUsuario)
                OUTPUT INSERTED.IdCarga
                VALUES
                    ('Carga de movimientos bancarios al ' + CONVERT(VARCHAR(20), GETDATE(), 100),
                     GETDATE(),
                     @RegistroUsuario);",
                new { RegistroUsuario = usuario },
                transaction: transaction)
                .FirstOrDefault();
        }

        private static StringBuilder ValidarChequesCarga(
            TesBancosCargadoChequesCambiadosObtenerRequest request,
            string usuario)
        {
            var errores = new StringBuilder();

            if (request is null || request.Cheques.Count == 0)
            {
                errores.AppendLine("Debe enviar al menos un cheque cambiado.");
                return errores;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                errores.AppendLine("El usuario de sesión es requerido.");
            }

            for (var i = 0; i < request.Cheques.Count; i++)
            {
                ValidarChequeCarga(request.Cheques[i], i + 1, errores);
            }

            return errores;
        }

        private static void ValidarChequeCarga(
            TesBancosCargadoChequeCambiadoCargaRequest cheque,
            int fila,
            StringBuilder errores)
        {
            if (cheque.Fecha is null)
                errores.AppendLine($"Fila {fila}: la fecha es obligatoria.");

            if (string.IsNullOrWhiteSpace(cheque.Documento))
                errores.AppendLine($"Fila {fila}: el documento es obligatorio.");

            if (string.IsNullOrWhiteSpace(cheque.NumeroCheque))
                errores.AppendLine($"Fila {fila}: el número de cheque es obligatorio.");

            if (cheque.Monto is null || cheque.Monto <= 0)
                errores.AppendLine($"Fila {fila}: el importe debe ser mayor a 0.");

            if (ObtenerIdBanco(cheque) <= 0)
                errores.AppendLine($"Fila {fila}: el id_banco debe contener el código del banco.");

            ValidarTipoMovimientoChequeCarga(cheque, fila, errores);
        }

        private static void ValidarTipoMovimientoChequeCarga(
            TesBancosCargadoChequeCambiadoCargaRequest cheque,
            int fila,
            StringBuilder errores)
        {
            var tipoMovimiento = cheque.TipoMovimiento?.Trim().ToUpperInvariant();
            if (tipoMovimiento != "C" && tipoMovimiento != "D")
                errores.AppendLine($"Fila {fila}: el tipo debe ser C o D.");
        }

        private static StringBuilder ValidarChequesProceso(
            TesBancosCargadoChequesCambiadosProcesarRequest request,
            string usuario)
        {
            var errores = new StringBuilder();

            if (request is null || request.Cheques.Count == 0)
            {
                errores.AppendLine("Debe enviar al menos un cheque para procesar.");
                return errores;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                errores.AppendLine("El usuario de sesión es requerido.");
            }

            for (var i = 0; i < request.Cheques.Count; i++)
            {
                var fila = i + 1;
                var cheque = request.Cheques[i];

                if (cheque.FechaTransaccion is null)
                    errores.AppendLine($"Fila {fila}: la fecha de transacción es obligatoria.");

                if (string.IsNullOrWhiteSpace(cheque.Documento))
                    errores.AppendLine($"Fila {fila}: el documento es obligatorio.");

                if (string.IsNullOrWhiteSpace(cheque.NumeroCheque))
                    errores.AppendLine($"Fila {fila}: el número de cheque es obligatorio.");

                if (cheque.Monto <= 0)
                    errores.AppendLine($"Fila {fila}: el monto debe ser mayor a 0.");

                if (cheque.Id_Banco <= 0)
                    errores.AppendLine($"Fila {fila}: el banco es obligatorio.");
            }

            return errores;
        }

        private static short ObtenerIdBanco(
            TesBancosCargadoChequeCambiadoCargaRequest cheque)
        {
            if (cheque.IdBanco is not null && cheque.IdBanco > 0)
            {
                return cheque.IdBanco.Value;
            }

            return short.TryParse(cheque.Cuenta?.Trim(), out var idBanco)
                ? idBanco
                : (short)0;
        }

        private static bool EsChequeProcesable(
            TesBancosCargadoChequeCambiadoDto cheque)
        {
            return cheque.Solicitud > 0
                && cheque.FechaTransaccion is not null
                && cheque.Id_Banco > 0
                && string.Equals(
                    cheque.Estado?.Trim(),
                    "NO ASOCIADO",
                    StringComparison.OrdinalIgnoreCase);
        }

        private sealed class TesBancosCargadoChequeProcesoResultadoDto
        {
            public string? descripcion { get; set; }
        }
    }
}
