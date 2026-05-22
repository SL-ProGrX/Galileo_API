using Dapper;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class MTesFuncionesDb
    {
        public static ErrorDto<object> SbTeBcrEmpresarialCore(
   SqlConnection conn,
   int CodEmpresa,
   int vBanco,
   string vTipoDoc,
   int cantidadSolicitudes,
   int? solInicio,
   int? solCorte,
   DateTime? fechaInicio,
   DateTime? fechaCorte,
   Func<long> resolveConsecutivo)
        {
            try
            {
                var (numNegocio, cedulaReg) = GetEmpresaNumNegocioYReg(conn);

                int bancoId = vBanco;
                string bancoTDoc = vTipoDoc;
                long bancoConsec = resolveConsecutivo();
                DateTime fecha = DateTime.Now;

                string conArchivo = GetConsecutivoArchivoDelDia(conn, bancoId, fecha)
                    .ToString("D3", CultureInfo.InvariantCulture);

                var sb = new StringBuilder();
                sb.AppendLine(BuildControlBcrEmpresarial(cedulaReg, conArchivo, fecha));

                var lineasDebito = conn.Query<string>(
                    "exec spTES_BCR_Empresarial 2, @banco, @bancoTDoc, @numNegocio, @bancoConsec, @cantidadSolicitudes, @mSolInicio, @mSolCorte, @mFechaInicio, @mFechaCorte",
                    new
                    {
                        banco = bancoId,
                        bancoTDoc,
                        numNegocio,
                        bancoConsec,
                        cantidadSolicitudes,
                        mSolInicio = solInicio,
                        mSolCorte = solCorte,
                        mFechaInicio = fechaInicio,
                        mFechaCorte = fechaCorte
                    });

                foreach (var linea in lineasDebito)
                    AppendIfNotEmpty(sb, linea);

                var lineasCredito = conn.Query<string>(
                    "exec spTES_BCR_Empresarial 3, @banco, @bancoTDoc, @numNegocio, @bancoConsec, @cantidadSolicitudes, @mSolInicio, @mSolCorte, @mFechaInicio, @mFechaCorte",
                    new
                    {
                        banco = bancoId,
                        bancoTDoc,
                        numNegocio,
                        bancoConsec,
                        cantidadSolicitudes = 100000,
                        mSolInicio = 0,
                        mSolCorte = 0,
                        mFechaInicio = (string?)null,
                        mFechaCorte = (string?)null
                    });

                foreach (var linea in lineasCredito)
                    AppendIfNotEmpty(sb, linea);

                return ArchivoResponse(bancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

    }
}
