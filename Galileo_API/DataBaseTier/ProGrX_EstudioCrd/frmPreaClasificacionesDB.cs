using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaClasificacionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaClasificacionesDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmPreaClasificacionesDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        public ErrorDto<List<PreaClasificacionRazonData>> PreaClasificacion_Razones_Obtener(int codEmpresa)
        {
            const string query = @"select cod_razon,descripcion,color 
                from Crd_Clasificacion_Razon order by cod_razon";
            return DbHelper.ExecuteListQuery<PreaClasificacionRazonData>(
                _portalDb, codEmpresa, query);
        }

        public ErrorDto<List<PreaClasificacionData>> PreaClasificacion_Catalogo_Obtener(int codEmpresa, string catalogo)
        {
            string query = "";
            switch(catalogo)
            {
                case "garantia":
                query = @"select 
                    A.cod_garantia as codigo,A.descripcion, rtrim(B.cod_Razon) + ' - ' + rtrim(B.descripcion) as razon 
                    from Crd_Clasificacion_Garantia A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon 
                    order by A.cod_Garantia";
                break;

                case "mora":
                query = @"select A.cod_mora as codigo, case 
                    when A.tipo = 'A' then 'Al Día'
                    when A.tipo = 'M' then 'Mora'
                    when A.tipo = 'C' then 'Cobro (Ejecutado)'
                    when A.tipo = 'I' then 'Incobrable' end as Tipo 
                    ,A.desde,A.hasta,rtrim(B.cod_Razon) + ' - ' + rtrim(B.descripcion) as razon 
                    from Cbr_Clasificacion_Mora A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon
                    order by A.cod_mora";
                 break;

                case "capacidad":
                query = @"select 
                    A.cod_capacidad as codigo,A.desde,A.hasta,rtrim(B.cod_Razon) + ' - ' + rtrim(B.descripcion) as razon 
                    from Crd_Clasificacion_Capacidad A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon
                    order by A.cod_capacidad";
                    break;

                case "endeudamiento":
                query = @"select 
                    A.cod_endeudamiento as codigo,A.desde,A.hasta,rtrim(B.cod_Razon) + ' - ' + rtrim(B.descripcion) as razon 
                    from Crd_Clasificacion_endeudamiento A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon
                    order by A.cod_endeudamiento";
                break;

                case "historial":
                query = @"select 
                    A.cod_historial as codigo,A.descripcion, rtrim(B.cod_Razon) + ' - ' + rtrim(B.descripcion) as razon 
                    from Crd_Clasificacion_historial A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon 
                    order by A.cod_historial";
                    break;

                default:
                    break;
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto<List<PreaClasificacionData>>
                {
                    Code = -1,
                    Description = "El catálogo solicitado no es válido.",
                    Result = new List<PreaClasificacionData>()
                };
            }

            return DbHelper.ExecuteListQuery<PreaClasificacionData>(
                _portalDb, codEmpresa, query);
        }

        public ErrorDto PreaClasificacion_Razon_Guardar(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            var resp = ExisteRazon(codEmpresa, request.cod_razon)
                ? ActualizarRazon(codEmpresa, usuario, request)
                : InsertarRazon(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        public ErrorDto PreaClasificacion_Razon_Eliminar(int codEmpresa, string codRazon, string usuario)
        {
            const string sqlDelete = @"delete Crd_Clasificacion_Razon where cod_razon = @CodRazon;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodRazon = codRazon
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"PreAnalisis (Razon) : {codRazon}"
            );

            return respDelete;
        }

        private bool ExisteRazon(int codEmpresa, string codRazon)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) as Existe 
            FROM Crd_Clasificacion_Razon WHERE cod_razon = @CodRazon;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodRazon = codRazon.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarRazon(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Clasificacion_Razon
            SET
                descripcion = @Descripcion,
                color = @Color
            WHERE cod_razon = @CodRazon;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodRazon = request.cod_razon?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Color = request.color?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"PreAnalisis (Razon) : {request.cod_razon}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarRazon(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Clasificacion_Razon
            (
                cod_razon,
                descripcion,
                color
            )
            VALUES
            (
                @CodRazon,
                @Descripcion,
                @Color
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodRazon = request.cod_razon?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Color = request.color?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"PreAnalisis (Razon) : {request.cod_razon}"
            );

            return respInsert;
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
