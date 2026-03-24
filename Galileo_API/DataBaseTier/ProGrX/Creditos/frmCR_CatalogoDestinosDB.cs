using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoDestinosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoDestinosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoDestinosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Obtener(int codEmpresa)
        {
            string query = @"select cod_destino,descripcion,tasa,tbp,int_form,
            case when isnull(TCIntForma,'A') = 'A' then 'Adelantado' else 'Vencido' end as 'TipoCbrInt', 
            primer_cuota,ENVIO_TESORERIA,prioridad from Catalogo_Destinos 
            order by cod_destino";
            return DbHelper.ExecuteListQuery<CrCatalogoDestinoData>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogos_Obtener(int codEmpresa, string tipo)
        {
            string query = @"select codigo as item,descripcion 
                from catalogo where (Retencion = @tipo and Poliza = @tipo)
                order by codigo";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { tipo });
        }

        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Asignados_Obtener(int codEmpresa, string codigo)
        {
            string query = @"SELECT R.*, 
                CASE 
                    WHEN A.codigo IS NOT NULL THEN 1
                    ELSE 0
                END AS Existe
            FROM Catalogo_Destinos R
            LEFT JOIN Catalogo_DestinosAsg A 
                ON R.cod_destino = A.cod_destino 
               AND A.codigo = @codigo
            ORDER BY Existe DESC, R.cod_destino;";
            return DbHelper.ExecuteListQuery<CrCatalogoDestinoData>(_portalDb, codEmpresa, query, new { codigo });
        }

        public ErrorDto CrCatalogoDestinos_Asignar(int codEmpresa, string codDestino, string catalogo, bool isChecked)
        {
            string sql;

            if (isChecked)
            {
                sql = @"
            INSERT INTO Catalogo_DestinosAsg (cod_destino, codigo)
            VALUES (@CodDestino, @Codigo);";
            }
            else
            {
                sql = @"
            DELETE FROM Catalogo_DestinosAsg
            WHERE cod_destino = @CodDestino
              AND codigo = @Codigo;";
            }

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodDestino = codDestino.Trim(),
                    Codigo = catalogo.Trim()
                }
            );
        }

        public ErrorDto CrCatalogoDestinos_Guardar(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            request.cod_destino = request.cod_destino.Trim();
            request.descripcion = request.descripcion?.Trim() ?? string.Empty;
            request.tipocbrint = request.tipocbrint?.Trim() ?? string.Empty;

            var existe = ExisteDestino(codEmpresa, request.cod_destino);

            var resp = existe
                ? ActualizarDestino(codEmpresa, usuario, request)
                : InsertarDestino(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        public ErrorDto CrCatalogoDestinos_Eliminar(int codEmpresa, string codDestino, string usuario)
        {
            const string sqlDelete = @"DELETE FROM Catalogo_Destinos
            WHERE cod_destino = @CodDestino;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodDestino = codDestino.Trim()
                }
            );

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Destino de Línea Cod: {codDestino}"
            );

            return respDelete;
        }

        private ErrorDto ActualizarDestino(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            const string sqlUpdate = @"
            UPDATE Catalogo_Destinos
            SET
                descripcion       = @Descripcion,
                tasa              = @Tasa,
                TBP               = @TBP,
                int_form          = @IntForm,
                TCIntForma        = @TipoCbrInt,
                primer_cuota      = @PrimerCuota,
                envio_tesoreria   = @EnvioTesoreria,
                prioridad         = @Prioridad
            WHERE cod_destino = @CodDestino;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodDestino = request.cod_destino,
                    Descripcion = request.descripcion,
                    Tasa = request.tasa,
                    TBP = request.tbp,
                    IntForm = request.int_form,
                    TipoCbrInt = request.tipocbrint,
                    PrimerCuota = request.primer_cuota,
                    EnvioTesoreria = request.envio_tesoreria,
                    Prioridad = request.prioridad
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Destino Línea Cod: {request.cod_destino}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarDestino(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            const string sqlInsert = @"
            INSERT INTO Catalogo_Destinos
            (
                cod_destino,
                descripcion,
                tasa,
                TBP,
                int_form,
                TCIntForma,
                primer_cuota,
                envio_tesoreria,
                prioridad
            )
            VALUES
            (
                @CodDestino,
                @Descripcion,
                @Tasa,
                @TBP,
                @IntForm,
                @TipoCbrInt,
                @PrimerCuota,
                @EnvioTesoreria,
                @Prioridad
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodDestino = request.cod_destino,
                    Descripcion = request.descripcion,
                    Tasa = request.tasa,
                    TBP = request.tbp,
                    IntForm = request.int_form,
                    TipoCbrInt = request.tipocbrint,
                    PrimerCuota = request.primer_cuota,
                    EnvioTesoreria = request.envio_tesoreria,
                    Prioridad = request.prioridad
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Destino de Línea Cod: {request.cod_destino}"
            );

            return respInsert;
        }

        private bool ExisteDestino(int codEmpresa, string codDestino)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) 
            FROM Catalogo_Destinos WHERE cod_destino = @CodDestino;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sqlExiste, 0,
                new
                {
                    CodDestino = codDestino.Trim()
                }
            );
            return resp.Result > 0;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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
