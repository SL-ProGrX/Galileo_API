using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    /// <summary>
    /// Carga del archivo Excel (VB6: sbArchivo_Load).
    /// </summary>
    public partial class FrmCrPolizaProcRecepcionDB
    {
        private const int MaxCharsPorBloque = 20000;
        private const string MensajeCargaOk = "Información Cargada Satisfactoriamente";

        /// <summary>
        /// Carga el archivo: valida el corte, crea el encabezado si no existe, inserta el detalle por bloques
        /// y recalcula el resumen del corte (VB6: sbArchivo_Load).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Archivo_Cargar(int codEmpresa, string usuario, PolizaArchivoCargarRequest request)
        {
            var errorRequest = ValidarCargaRequest(request);
            if (errorRequest is not null)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(errorRequest);
            }

            var datos = request.datos!;
            var corte = datos.corte!.Value;

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();

                var valida = ResultadoSp(SpCorteValida(conn, corte, datos.cod_poliza, datos.id_factura ?? 0));
                if (valida.Code == -1)
                {
                    return valida;
                }

                var maestro = ResolverMaestroCarga(conn, datos, usuario);
                if (maestro.Code == -1)
                {
                    return maestro;
                }

                var idFactura = maestro.Result!.idfactura ?? 0;
                CargarDetalle(conn, idFactura, request.filas, usuario);
                SpCorteActualiza(conn, corte, datos.cod_poliza, idFactura);

                return DbHelper.CreateOkResponse<PolizaAseguradoraCorte?>(new PolizaAseguradoraCorte
                {
                    pass = 1,
                    mensaje = MensajeCargaOk,
                    idfactura = idFactura
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaAseguradoraCorte?>(ex.Message);
            }
        }

        /// <summary>
        /// Valida el request de carga. Devuelve el mensaje de error o null si es válido.
        /// </summary>
        private static string? ValidarCargaRequest(PolizaArchivoCargarRequest? request)
        {
            var errorEncabezado = ValidarEncabezado(request?.datos);
            if (errorEncabezado is not null)
            {
                return errorEncabezado;
            }

            return request!.filas.Count == 0
                ? "El archivo no contiene datos válidos."
                : null;
        }

        /// <summary>
        /// Crea el encabezado de la factura cuando aún no existe (id_factura = 0); si existe lo reutiliza.
        /// </summary>
        private static ErrorDto<PolizaAseguradoraCorte?> ResolverMaestroCarga(SqlConnection conn, PolizaAseguradoraCorteData datos, string usuario)
        {
            var idActual = datos.id_factura ?? 0;
            if (idActual != 0)
            {
                return DbHelper.CreateOkResponse<PolizaAseguradoraCorte?>(new PolizaAseguradoraCorte { pass = 1, idfactura = idActual });
            }

            return ResultadoSp(SpCorteAdd(conn, datos, usuario));
        }

        /// <summary>
        /// Inserta el detalle en bloques dentro de una transacción. Se omiten las filas sin cédula (VB6: Trim(Cedula) &lt;&gt; "").
        /// </summary>
        private static int CargarDetalle(SqlConnection conn, int idFactura, IEnumerable<PolizaAseguradoraCorteDetalleData> filas, string usuario)
        {
            using var tran = conn.BeginTransaction();
            try
            {
                var lote = new LoteDetalle();

                foreach (var fila in filas.Where(f => !string.IsNullOrWhiteSpace(f.cedula)))
                {
                    lote.Agregar(fila, idFactura, usuario);

                    if (lote.Sql.Length > MaxCharsPorBloque)
                    {
                        lote.Ejecutar(conn, tran);
                    }
                }

                lote.Ejecutar(conn, tran);
                tran.Commit();
                return lote.Lineas;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Acumula instrucciones EXEC parametrizadas de spPoliza_Aseguradora_Corte_Detalle_Add.
        /// Inicializa = 1 solo en la primera línea (igual que VB6).
        /// </summary>
        private sealed class LoteDetalle
        {
            public StringBuilder Sql { get; } = new();
            public int Lineas { get; private set; }

            private DynamicParameters _parametros = new();

            public void Agregar(PolizaAseguradoraCorteDetalleData fila, int idFactura, string usuario)
            {
                Lineas++;
                var s = "_" + Lineas;

                _parametros.Add("@f" + s, idFactura, DbType.Int32);
                _parametros.Add("@l" + s, Lineas, DbType.Int32);
                _parametros.Add("@c" + s, fila.cedula.Trim(), DbType.String);
                _parametros.Add("@n" + s, (fila.nombre ?? string.Empty).Trim(), DbType.String);
                _parametros.Add("@p" + s, (fila.n_poliza ?? string.Empty).Trim(), DbType.String);
                _parametros.Add("@m" + s, fila.monto_asegurado ?? 0m, DbType.Decimal);
                _parametros.Add("@pr" + s, fila.prima ?? 0m, DbType.Decimal);
                _parametros.Add("@o" + s, fila.operacion ?? 0L, DbType.Int64);
                _parametros.Add("@u" + s, usuario, DbType.String);
                _parametros.Add("@i" + s, Lineas == 1 ? 1 : 0, DbType.Int32);

                Sql.Append("EXEC dbo.spPoliza_Aseguradora_Corte_Detalle_Add ")
                   .Append($"@f{s}, @l{s}, @c{s}, @n{s}, @p{s}, @m{s}, @pr{s}, @o{s}, @u{s}, @i{s};")
                   .AppendLine();
            }

            public void Ejecutar(SqlConnection conn, SqlTransaction tran)
            {
                if (Sql.Length == 0)
                {
                    return;
                }

                conn.Execute(Sql.ToString(), _parametros, transaction: tran, commandType: CommandType.Text);
                Sql.Clear();
                _parametros = new DynamicParameters();
            }
        }
    }
}
