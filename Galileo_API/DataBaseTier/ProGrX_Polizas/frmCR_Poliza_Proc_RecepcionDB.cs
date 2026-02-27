using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaProcRecepcionDB
    {
        private readonly PortalDB _portalDb; 
        public FrmCrPolizaProcRecepcionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
          
        }

        /// <summary>
        /// Lista las pólizas disponibles para el proceso de recepción, sin filtros adicionales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaProcRecepcion_Listar(int codEmpresa)
        {
            var query = @"
                SELECT Cp.COD_POLIZA as item,
                       Cp.DESCRIPCION as descripcion
                FROM CRD_CATALOGO_POLIZAS Cp
                ORDER BY Cp.DESCRIPCION";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Lista las unidades activas para la contabilidad indicada, que pueden ser relevantes para el proceso de recepción de pólizas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaUnidades_Listar(int codEmpresa, int codContabilidad)
        {
            var query = @"
                SELECT rtrim(cod_unidad) as item,
                      rtrim(descripcion) as descripcion
               from CntX_unidades where cod_contabilidad =@codContabilidad and activa = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codContabilidad });
        }

        /// <summary>
        /// Lista las pólizas que están marcadas como facturable.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaFacturables_Listar(int codEmpresa)
        {
            var query = @" exec spPoliza_Facturables_Lista";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Lista los centros de costo activos para la contabilidad y unidad indicadass.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codUnidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaCentrosCosto_Listar(int codEmpresa,int codContabilidad,string codUnidad)
        {
            var query = @"
                SELECT RTRIM(COD_CENTRO_COSTO) as item,
                           RTRIM(descripcion)  as descripcion
               From CNTX_CENTRO_COSTOS 
                    Where COD_CONTABILIDAD = @codContabilidad
                    And ACTIVO = 1
                    and COD_CENTRO_COSTO in(select COD_CENTRO_COSTO  from CNTX_UNIDADES_CC
                    where COD_CONTABILIDAD = @codContabilidad
                     and COD_UNIDAD = @codUnidad
                    ";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codContabilidad, codUnidad });
        }

        /// <summary>
        /// Lista las divisas disponibles para la contabilidad indicada, marcando la divisa local para facilitar su identificación en el proceso de recepción de pólizas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> PolizaDivisas_Listar(int codEmpresa, int codContabilidad)
        {
            var query = @"
                SELECT rtrim(cod_divisa) as item,
                    rtrim(descripcion) as descripcion
              from CntX_Divisas where cod_contabilidad =@codContabilidad
                    order by divisa_local desc,cod_divisa
                    ";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codContabilidad });
        }

        /// <summary>
        /// Consulta la divisa local para la contabilidad indicada.
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ErrorDto<DropDownListaGenericaModel> PolizaDivisasLocal_Consulta (int codEmpresa, int codContabilidad)
        {
            var query = @"
               select rtrim(cod_divisa) as 'Divisa',rtrim(descripcion) as 'DivisaLocal'
                    from CntX_Divisas where cod_contabilidad =@codContabilidad
                    and Divisa_Local = 1
                    ";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<DropDownListaGenericaModel>(
                    query,
                    new { codContabilidad });

                return result is null
                    ? throw new InvalidOperationException(
                        "No existe una divisa local para la contabilidad indicada.")
                    : result;
            });
        }

        /// <summary>
        /// Valida si ya existe una póliza para el corte, póliza y factura indicados, para evitar duplicados en el proceso de recepción de pólizas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <param name="idFactura"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Valida(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        {

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                    "spPoliza_Aseguradora_Corte_Valida",
                    new
                    {
                        corte,
                        codPoliza,
                        idFactura
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }

        /// <summary>
        /// Agrega una nueva póliza de aseguradora para el corte, póliza y factura indicados, registrando el usuario que realiza la operación para fines de auditoría en el proceso de recepción de pólizas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Agregar(int codEmpresa, string usuario, PolizaAseguradoraCorteData datos)
        {

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                    "spPoliza_Aseguradora_Corte_Add",
                    new
                    {
                        datos.Corte,
                        datos.CodPoliza,
                        datos.IdFactura,
                        datos.AseguradoraId,
                        datos.ProveedorId,
                        datos.Factura,
                        datos.FormaPago,
                        datos.Vence,
                        datos.Divisa,
                        datos.Unidad,
                        datos.CentroCosto,
                        datos.TipoCambio,
                        datos.Notas,
                        usuario
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }

        /// <summary>
        /// Agrega los detalles de la póliza de aseguradora para el corte, póliza y factura indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="scFacturaId"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto<int> PolizaAseguradoraCorteDetalle_Agregar(int codEmpresa, string usuario, int scFacturaId, IEnumerable<PolizaAseguradoraCorteDetalleData> datos)
        {
            int maxCharsPorBloque = 20000;
            if (datos == null)
                return DbHelper.CreateErrorResponse<int>("No se recibieron filas para procesar.");


            return DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                conn.Open();
                using var tran = conn.BeginTransaction();

                var sb = new StringBuilder(capacity: Math.Min(maxCharsPorBloque + 1024, 1 << 20));
                var batchParams = new DynamicParameters();

                int totalAfectadas = 0;
                int linea = 0;      // pLinea en VB6, incrementa solo en filas válidas
                int filaIdx = 0;    // sufijo único para parámetros de cada fila
                bool primeraMarcada = false;

                try
                {
                    foreach (var row in datos)
                    {
                        var cedula = (row.Cedula ?? string.Empty).Trim();
                        if (cedula.Length == 0)
                            continue;

                        filaIdx++;
                        linea++;

                        // Inicializa = 1 solo para la primera fila válida
                        int inicializa = (!primeraMarcada && linea == 1) ? 1 : 0;
                        if (inicializa == 1) primeraMarcada = true;


                        var args = new AppendExecArgs
                        {
                            Suf = "_" + filaIdx,
                            IdFactura = scFacturaId,
                            Linea = linea,
                            Cedula = cedula,
                            Row = row,
                            Usuario = usuario,
                            Inicializa = inicializa
                        };
                        AppendExecForRow(sb, batchParams, args);


                        totalAfectadas += FlushIfNeeded(conn, tran, sb, ref batchParams, maxCharsPorBloque);
                    }

                    totalAfectadas += FlushIfNeeded(conn, tran, sb, ref batchParams, force: true);

                    tran.Commit();
                    return totalAfectadas;

                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            });

        }

        /// <summary>
        /// Construye la línea EXEC para agregar un detalle de póliza de aseguradora, con parámetros únicos por fila para evitar colisiones en el batch, y agrega los parámetros al objeto DynamicParameters.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="batchParams"></param>
        /// <param name="args"></param>
        static void AppendExecForRow(StringBuilder sb, DynamicParameters batchParams, AppendExecArgs args)
        {


            // Parámetros (todos con sufijo único)
            batchParams.Add("@IdFactura" + args.Suf, args.IdFactura, DbType.Int32);
            batchParams.Add("@IdLinea" + args.Suf, args.Linea, DbType.Int32);
            batchParams.Add("@Cedula" + args.Suf, args.Cedula, DbType.String);
            batchParams.Add("@Nombre" + args.Suf, (args.Row.Nombre ?? string.Empty).Trim(), DbType.String);
            batchParams.Add("@NumPoliza" + args.Suf, (args.Row.NumPoliza ?? string.Empty).Trim(), DbType.String);
            batchParams.Add("@MontoAsegurado" + args.Suf, args.Row.MontoAsegurado, DbType.Decimal);
            batchParams.Add("@Prima" + args.Suf, args.Row.Prima, DbType.Decimal);
            batchParams.Add("@Operacion" + args.Suf, args.Row.Operacion, DbType.Int32);
            batchParams.Add("@Usuario" + args.Suf, args.Usuario, DbType.String);
            batchParams.Add("@Inicializa" + args.Suf, args.Inicializa, DbType.Int32);

            // Línea EXEC con los nombres de TU SP y TUS parámetros (parametrizado)
            sb.Append(' ', 10);
            sb.Append("EXEC dbo.spPoliza_Aseguradora_Corte_Detalle_Add ");
            sb.Append("@IdFactura").Append(args.Suf).Append(", ");
            sb.Append("@IdLinea").Append(args.Suf).Append(", ");
            sb.Append("@Cedula").Append(args.Suf).Append(", ");
            sb.Append("@Nombre").Append(args.Suf).Append(", ");
            sb.Append("@NumPoliza").Append(args.Suf).Append(", ");
            sb.Append("@MontoAsegurado").Append(args.Suf).Append(", ");
            sb.Append("@Prima").Append(args.Suf).Append(", ");
            sb.Append("@Operacion").Append(args.Suf).Append(", ");
            sb.Append("@Usuario").Append(args.Suf).Append(", ");
            sb.Append("@Inicializa").Append(args.Suf).Append(';');
            sb.AppendLine();

        }

        /// <summary>
        /// Ejecuta el batch acumulado en el StringBuilder si se ha superado el límite de caracteres o si se fuerza la ejecución, y limpia el StringBuilder y los parámetros para el siguiente batch.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tran"></param>
        /// <param name="sb"></param>
        /// <param name="batchParams"></param>
        /// <param name="maxCharsPorBloque"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        static int FlushIfNeeded(
            SqlConnection conn,
            SqlTransaction tran,
            StringBuilder sb,
            ref DynamicParameters batchParams,
            int maxCharsPorBloque = 20000,
            bool force = false)
        {
            if (!force && sb.Length <= maxCharsPorBloque) return 0;
            if (sb.Length == 0) return 0;

            var afectadas = conn.Execute(
                sb.ToString(),
                batchParams,
                transaction: tran,
                commandType: CommandType.Text);

            sb.Clear();
            batchParams = new DynamicParameters();
            return afectadas;
        }

        /// <summary>
        /// Registra el pago de una póliza de aseguradora para el corte, póliza y factura indicados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <param name="idFactura"></param>
        /// <returns></returns>
        public ErrorDto<PolizaAseguradoraCorte> PolizaAseguradoraCorte_Pago(int codEmpresa, string usuario, DateTime corte, string codPoliza, int idFactura)
        {

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizaAseguradoraCorte>(
                    "spPoliza_Aseguradora_Corte_Pago",
                    new
                    {
                        corte,
                        codPoliza,
                        idFactura,
                        usuario
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }

        /// <summary>
        /// Consulta el tipo de cambio para la contabilidad y divisa indicados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <param name="divisa"></param>
        /// <returns></returns>
        public ErrorDto<decimal> TipoCambio_Consultar(int codEmpresa, int contabilidad, string divisa)
        {
            const string sql = "select dbo.fxCntXTipoCambio(@contabilidad, @divisa, dbo.MyGetdate(), 'V')";

            var resp = DbHelper.ExecuteSingleQuery<decimal>(
                   _portalDb,
                   codEmpresa,
                   sql,
                   defaultValue: 0m,
                   parameters: new { contabilidad, divisa }
               );

            return resp;
        }

        /// <summary>
        /// Consulta los datos principales de la póliza para el proceso de recepción
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codPoliza"></param>
        /// <returns></returns>
        public ErrorDto<PolizaDatos> PolizaPolizaDatos(int codEmpresa, string codPoliza)
        {

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizaDatos>(
                    "spPoliza_Poliza_Datos",
                    new
                    {
                        codPoliza
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }

        /// <summary>
        /// Consulta los detalles de la póliza de aseguradora para el corte, póliza y factura indicados, para mostrar en el proceso de recepción de pólizas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <param name="idFactura"></param>
        /// <returns></returns>
        public ErrorDto<List<PolizaAseguradoraCorteDetalleConsulta>> PolizaAseguradoraCorteDetalle_Consulta(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
        {

            var sql = "spPoliza_Aseguradora_Corte_Detalle_Consulta";
            var parameters = new { corte, codPoliza, idFactura };
            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<PolizaAseguradoraCorteDetalleConsulta>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
            return DbHelper.CreateOkResponse(result.AsList());
    
        }

        /// <summary>
        /// Consulta los datos principales de la póliza para el proceso de recepción, filtrando por corte, póliza y factura.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="corte"></param>
        /// <param name="codPoliza"></param>
        /// <returns></returns>
        public ErrorDto<PolizaDatos> PolizaAseguradoraCorte_Consulta(int codEmpresa, DateTime corte, string codPoliza)
        {

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var result = conn.QueryFirstOrDefault<PolizaDatos>(
                    "spPoliza_Aseguradora_Corte_Consulta",
                    new
                    {
                        corte,
                        codPoliza
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
                return result!;
            });
        }


    }
}
