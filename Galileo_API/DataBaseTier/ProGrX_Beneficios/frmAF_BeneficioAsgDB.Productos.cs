using Dapper;
using Galileo.Models.AF;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioAsgDB
    {
        private const string BeneficioProductoInsertarSql = @"INSERT afi_bene_prodasg (consec, cod_beneficio, cod_producto, cantidad, costo_unidad)
                                                               VALUES (@consec, @codBeneficio, @codProducto, @cantidad, @costoUnidad)";

        /// <summary>
        /// Persiste el encabezado y los productos asignados dentro de una única transacción.
        /// </summary>
        private static long AF_BeneficioAsg_Productos_Procesar(IDbConnection connection, BeneficioProductosGuardarRequest solicitud)
        {
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            var consecutivo = AF_BeneficioAsg_Productos_Consecutivo_Obtener(connection, transaction, solicitud);
            var filas = connection.Execute(
                solicitud.EsNuevo ? BeneficioOtorgaInsertarSql : BeneficioOtorgaActualizarSql,
                AF_BeneficioAsg_Productos_Parametros_Obtener(solicitud, consecutivo),
                transaction);

            if (filas <= 0)
            {
                transaction.Rollback();
                return 0L;
            }

            if (!solicitud.EsNuevo)
            {
                connection.Execute(BeneficioProductosEliminarSql, new
                {
                    codBeneficio = solicitud.Datos.cod_beneficio,
                    consec = consecutivo
                }, transaction);
            }

            if (!AF_BeneficioAsg_Productos_Insertar(connection, transaction, solicitud, consecutivo))
            {
                transaction.Rollback();
                return 0L;
            }

            transaction.Commit();
            return consecutivo;
        }

        private static long AF_BeneficioAsg_Productos_Consecutivo_Obtener(
            IDbConnection connection,
            IDbTransaction transaction,
            BeneficioProductosGuardarRequest solicitud)
        {
            return solicitud.EsNuevo
                ? connection.QueryFirstOrDefault<long>(
                    @"SELECT ISNULL(MAX(consec), 0) + 1
                        FROM afi_bene_otorga WITH (UPDLOCK, HOLDLOCK)
                       WHERE cod_beneficio = @codBeneficio",
                    new { codBeneficio = solicitud.Datos.cod_beneficio }, transaction)
                : solicitud.Datos.consec ?? 0L;
        }

        private static object AF_BeneficioAsg_Productos_Parametros_Obtener(BeneficioProductosGuardarRequest solicitud, long consecutivo)
        {
            var datos = solicitud.Datos;
            return new
            {
                consec = consecutivo,
                codBeneficio = datos.cod_beneficio,
                cedula = (datos.cedula ?? string.Empty).Trim(),
                monto = datos.monto,
                modificaMonto = solicitud.ModificaMonto,
                usuario = solicitud.Usuario.ToUpperInvariant(),
                estado = datos.estado,
                notas = datos.notas,
                solicita = datos.solicita,
                nombre = (datos.solicita_nombre ?? string.Empty).ToUpperInvariant(),
                tipo = datos.tipoBeneficio
            };
        }

        private static bool AF_BeneficioAsg_Productos_Insertar(
            IDbConnection connection,
            IDbTransaction transaction,
            BeneficioProductosGuardarRequest solicitud,
            long consecutivo)
        {
            foreach (var producto in solicitud.Datos.productos ?? [])
            {
                var filas = connection.Execute(BeneficioProductoInsertarSql, new
                {
                    consec = consecutivo,
                    codBeneficio = solicitud.Datos.cod_beneficio,
                    codProducto = producto.cod_producto,
                    cantidad = producto.cantidad,
                    costoUnidad = producto.costo_unidad
                }, transaction);

                if (filas <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private const string BeneficioOtorgaInsertarSql = @"
            INSERT afi_bene_otorga (consec, cod_beneficio, cedula, monto, modifica_monto, registra_user, registra_fecha,
                                    estado, notas, Solicita, nombre, tipo)
            VALUES (@consec, @codBeneficio, @cedula, @monto, @modificaMonto, @usuario, GETDATE(),
                    @estado, @notas, @solicita, @nombre, @tipo)";

        private const string BeneficioOtorgaActualizarSql = @"
            UPDATE afi_bene_otorga
               SET notas = @notas, estado = @estado, modifica_monto = @modificaMonto, solicita = @solicita,
                   monto = @monto, nombre = @nombre, TIPO = @tipo
             WHERE cod_beneficio = @codBeneficio AND cedula = @cedula AND consec = @consec";

        private const string BeneficioProductosEliminarSql = @"DELETE FROM afi_bene_prodasg
                                                                WHERE cod_beneficio = @codBeneficio AND consec = @consec";

        private sealed class BeneficioProductosGuardarRequest
        {
            public AfiBeneficioAsgInsertar Datos { get; init; } = new();
            public string ModificaMonto { get; init; } = string.Empty;
            public string Usuario { get; init; } = string.Empty;
            public bool EsNuevo { get; init; }
        }
    }
}
