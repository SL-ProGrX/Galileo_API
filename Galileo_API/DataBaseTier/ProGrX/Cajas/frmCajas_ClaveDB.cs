using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ProGrX.Cajas;
using System.Text;

namespace PgxAPI.DataBaseTier
{
    public class FrmCajasClaveDb
    {
        private readonly IConfiguration _config;

        public FrmCajasClaveDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<CajasUsuarioDto>> Cajas_Usuario_Obtener(int CodEmpresa,string usuario)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<CajasUsuarioDto>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<CajasUsuarioDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
            SELECT 
                C.cod_caja AS codigo,
                C.Descripcion AS descripcion,
                C.PERIOCIDAD_CONTRASENA AS periodicidad_contrasena
            FROM cajas_definicion C
            INNER JOIN cajas_usuarios U
                ON C.cod_caja = U.cod_caja
               AND U.usuario = @usuario
            WHERE C.Activa = 1
            ORDER BY C.cod_caja";

                response.Result = cn
                    .Query<CajasUsuarioDto>(sql, new { usuario })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasUsuarioDto>();
            }

            return response;
        }

        public ErrorDto<bool> Cajas_Cambio_Clave(int CodEmpresa,string usuario,string claveActual,
            string claveNueva,List<string> cajas)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Cambio de clave realizado correctamente",
                Result = true
            };

            try
            {
                using var cn = new SqlConnection(connString);
                cn.Open();
                using var tx = cn.BeginTransaction();

                string claveActualCifrada = FxStringCifrado(claveActual.Trim());

                int existe = cn.ExecuteScalar<int>(
                    @"SELECT COUNT(1) FROM cajas_usuarios WHERE usuario = @usuario AND contrasena = @clave",
                    new
                    {
                        usuario,
                        clave = claveActualCifrada
                    },
                    tx
                );

                // Reemplace la excepción genérica por una excepción específica
                if (existe == 0)
                    throw new InvalidOperationException(
                        "La clave del sistema digitada no corresponde a su usuario."
                    );
                string claveNuevaCifrada = FxStringCifrado(claveNueva.Trim());


                foreach (var codCaja in cajas)
                {

                    int periodicidad = cn.ExecuteScalar<int>(
                        @"SELECT ISNULL(PERIOCIDAD_CONTRASENA, 0)
                    FROM cajas_usuarios
                   WHERE cod_caja = @codCaja
                     AND usuario = @usuario",
                        new { codCaja, usuario },
                        tx
                    );

                    cn.Execute(
                        @"UPDATE cajas_usuarios
                     SET contrasena = @claveNueva,
                         Contrasena_Renovacion = DATEADD(
                             DAY,
                             @periodicidad,
                             dbo.MyGetdate()
                         )
                   WHERE cod_caja = @codCaja
                     AND usuario = @usuario",
                        new
                        {
                            claveNueva = claveNuevaCifrada,
                            periodicidad,
                            codCaja,
                            usuario
                        },
                        tx
                    );
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }


        private static string FxStringCifrado(string input)
        {
            var vResBuilder = new StringBuilder();
            var vResXBuilder = new StringBuilder();
            int vSec = 0;

            foreach (char c in input)
            {
                int ascii = (int)c;
                vResBuilder.Insert(0, ascii.ToString());
            }
            string vRes = vResBuilder.ToString();

            for (int i = 0; i < vRes.Length; i += 3)
            {
                int take = Math.Min(3, vRes.Length - i);
                string slice = vRes.Substring(i, take);
                int block = int.Parse(slice);
                int transformed = block;

                switch (vSec)
                {
                    case 0: transformed = block + 1; break;
                    case 1: transformed = block - 5; break;
                    case 2: transformed = block + 7; break;
                    case 3: transformed = block - 13; break;
                    case 4: transformed = block - 2; break;
                    case 5: transformed = block + 3; break;
                }

                vResXBuilder.Append(transformed.ToString());
                vSec = (vSec + 1) % 6;
            }

            return FxDepuraCadena(vResXBuilder.ToString());
        }

        private static string FxDepuraCadena(string cadena)
        {
            var vResBuilder = new StringBuilder();

            for (int i = 0; i < cadena.Length - 1; i++)
            {
                string sub = cadena.Substring(i, 2);

                if (int.TryParse(sub, out int num) && num > 31 && num != 39 && num != 34)
                {
                    vResBuilder.Insert(0, (char)num);
                }
            }

            return vResBuilder.ToString();
        }
    }


}

