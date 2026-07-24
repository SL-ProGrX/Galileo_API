using Dapper;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    internal static class FrmTesAutorizacionesLotesDB
    {
        private const int TamanoLoteInsercion = 1000;

        /// <summary>
        /// Inserta las solicitudes en lotes para los procesos de autorización y desautorización.
        /// </summary>
        internal static void TES_Autorizaciones_InsertarSolicitudes(
            IDbConnection conn,
            IEnumerable<int> solicitudes,
            string estado,
            string usuario)
        {
            foreach (int[] lote in solicitudes.Chunk(TamanoLoteInsercion))
            {
                TES_Autorizaciones_InsertarLote(
                    conn,
                    lote,
                    estado,
                    usuario);
            }
        }

        /// <summary>
        /// Inserta un lote de solicitudes en la tabla de autorización masiva.
        /// </summary>
        private static void TES_Autorizaciones_InsertarLote(
            IDbConnection conn,
            IReadOnlyList<int> solicitudes,
            string estado,
            string usuario)
        {
            var sql = new StringBuilder(
                "INSERT INTO TES_MASS_AUTORIZACION " +
                "(NSOLICITUD, ESTADO, USUARIO) VALUES ");

            var parametros = new DynamicParameters();

            parametros.Add("Estado", estado);
            parametros.Add("Usuario", usuario);

            int indice = 0;

            foreach (int solicitud in solicitudes)
            {
                if (indice > 0)
                {
                    sql.Append(',');
                }

                string nombreParametro = $"Solicitud{indice}";

                sql.Append($"(@{nombreParametro}, @Estado, @Usuario)");
                parametros.Add(nombreParametro, solicitud);
                indice++;
            }

            conn.Execute(sql.ToString(), parametros, commandTimeout: 0);
        }
    }
}
