using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;


namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficiosDB
    {
        private readonly IConfiguration _config;
        mSecurityMainDb DBBitacora;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public frmAF_BeneficiosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
            _mBeneficiosDB = new mBeneficiosDB(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto Top1Beneficio_Obtener(int CodCliente, int Scroll, string Cod_Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Top 1 Cod_Beneficio from afi_beneficios";

                    if (!string.IsNullOrEmpty(Cod_Beneficio))
                    {
                        if (Scroll == 1)
                        {
                            query += $" where Cod_Beneficio > '{Cod_Beneficio}' order by Cod_Beneficio asc";
                        }
                        else
                        {
                            query += $" where Cod_Beneficio < '{Cod_Beneficio}' order by Cod_Beneficio desc";
                        }
                    }

                    info.Description = connection.QueryFirstOrDefault<string>(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto<AfiBeneficiosDTO> AfiBeneficioDTO_Obtener(int CodCliente, string Cod_Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AfiBeneficiosDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select * from afi_beneficios where Cod_Beneficio = '{Cod_Beneficio}'";
                    response.Result = connection.Query<AfiBeneficiosDTO>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto AfiBeneGruposB_Insertar(int CodCliente, string cod_grupo, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert AFI_BENE_GRUPOSB(cod_grupo,cod_beneficio) values ('{cod_grupo}', '{cod_beneficio}')";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto AfiBeneGruposB_Eliminar(int CodCliente, string cod_grupo, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete AFI_BENE_GRUPOSB where cod_grupo = '{cod_grupo}' and cod_beneficio = '{cod_beneficio}'";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto<List<AfiBeneficioMontoData>> AfiBeneficioMontos_Obtener(int CodCliente, string Cod_Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneficioMontoData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select id_bene,inicio,corte,monto from afi_beneficio_montos 
                                    where cod_beneficio = '{Cod_Beneficio}'";
                    response.Result = connection.Query<AfiBeneficioMontoData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<List<AfiBeneficioGruposData>> AfiBeneficioGrupos_Obtener(int CodCliente, string Cod_Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneficioGruposData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select B.cod_grupo as 'Grupo',B.descripcion, case when A.cod_grupo is not null then 1 else 0 end as cod_grupo 
                                     from AFI_BENEFICIO_GRUPOS  B left join AFI_BENE_GRUPOSB A on B.cod_grupo = A.cod_grupo
                                     and  A.cod_beneficio = '{Cod_Beneficio}' 
                                     order by A.cod_grupo desc,B.descripcion asc";
                    response.Result = connection.Query<AfiBeneficioGruposData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto AfiBeneficios_Actualiza(int CodCliente, AfiBeneficiosDTO Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {


                    var query1 = $@" SELECT vigencia_meses, ESTADO, NOTAS, COD_CATEGORIA, COD_GRUPO
                    FROM Afi_beneficios  WHERE COD_BENEFICIO = '{Beneficio.cod_beneficio}'";
                    var result = connection.QueryFirstOrDefault(query1);

                    int VIGENCIA_MESES_ANTERIOR = (result.vigencia_meses== null)?0: result.vigencia_meses;
                    string ESTADO_ANTERIOR = result.ESTADO;
                    string NOTAS_ANTERIOR = result.NOTAS;
                    string COD_CATEGORIA_ANTERIOR = (result.COD_CATEGORIA == null)? "": result.COD_CATEGORIA;
                    string COD_GRUPO_ANTERIOR = ( result.COD_GRUPO == null)? "": result.COD_GRUPO.ToString();

                    var query = $@"update Afi_beneficios set descripcion = '{Beneficio.descripcion}'
                                     ,notas = '{Beneficio.notas}',estado = '{Beneficio.estado}',
                                     aplica_beneficiarios = {Beneficio.aplica_beneficiarios} ,modifica_monto = {Beneficio.modifica_monto}
                                     ,cod_cuenta = '{Beneficio.cod_cuenta}',tipo = '{Beneficio.tipo}'
                                     ,modifica_diferencia = {Beneficio.modifica_diferencia},maximo_otorga = {Beneficio.maximo_otorga}
                                     ,aplica_parcial = {Beneficio.aplica_parcial} 
                                     ,tipo_monetario = {Beneficio.tipo_monetario} ,tipo_producto = {Beneficio.tipo_producto} ,
                                      i_morosidad = {Beneficio.i_morosidad}, i_condicion_especial = {Beneficio.i_condicion_especial},
                                        i_suspendidos = {Beneficio.i_suspendidos}, i_insolventes = {Beneficio.i_insolventes}, 
                                    i_cobro_judicial = {Beneficio.i_cobro_judicial},
                                      Cod_Categoria = '{Beneficio.cod_categoria}' ,Cod_Grupo = '{Beneficio.cod_grupo}' ,VIGENCIA_MESES = {Beneficio.vigencia_meses}, PAGOS_MULTIPLES = {Beneficio.pagos_multiples} 
                                      where cod_beneficio = '{Beneficio.cod_beneficio}' ";
                    connection.Execute(query);

                    query = $@"delete afi_Grupo_Beneficio where cod_beneficio = '{Beneficio.cod_beneficio}'";
                    connection.Execute(query);

                    query = $@"insert into afi_Grupo_Beneficio (cod_beneficio,cod_grupo) values ('{Beneficio.cod_beneficio}', {Beneficio.cod_grupo} ) ";
                    connection.Execute(query);


                    if (Beneficio.vigencia_meses != VIGENCIA_MESES_ANTERIOR)
                    {
                        string detalleVigencia = $@"Actualiza vigencia del Beneficio de [{VIGENCIA_MESES_ANTERIOR} meses] por [{Beneficio.vigencia_meses} meses]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleVigencia, Beneficio.cod_beneficio, Beneficio.registra_user);
                    }

                    // Verificar si hay un cambio en 'estado' y registrar en la bitácora si es necesario
                    if (Beneficio.estado != ESTADO_ANTERIOR)
                    {
                        // Actualizar el estado a su representación textual
                        Beneficio.estado = (Beneficio.estado == "A") ? "Activo" : "Inactivo";
                        ESTADO_ANTERIOR = (ESTADO_ANTERIOR == "A") ? "Activo" : "Inactivo";

                        string detalleEstado = $@"Actualiza estado del Beneficio de [{ESTADO_ANTERIOR}] por [{Beneficio.estado}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);
                    }

                    // Verificar si hay un cambio en 'notas' y registrar en la bitácora si es necesario
                    if (Beneficio.notas != NOTAS_ANTERIOR)
                    {
                        string detalleEstado = $@"Actualiza las notas a: {Beneficio.notas}";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);
                    }

                    // Verificar si hay un cambio en 'categoria' y registrar en la bitácora si es necesario
                    if (Beneficio.cod_categoria != COD_CATEGORIA_ANTERIOR)
                    {
                        string detalleEstado = $@"Actualiza categoria del Beneficio de [{COD_CATEGORIA_ANTERIOR}] por [{Beneficio.cod_categoria}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);
                    }

                    // Verificar si hay un cambio en 'grupo' y registrar en la bitácora si es necesario
                    if (Beneficio.cod_grupo != COD_GRUPO_ANTERIOR)
                    {
                        string detalleEstado = $@"Actualiza grupo del Beneficio de [{COD_GRUPO_ANTERIOR}] por [{Beneficio.cod_grupo}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);
                    }



                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto AfiBeneficios_Insertar(int CodCliente, AfiBeneficiosDTO Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert into afi_beneficios (
                                        cod_beneficio,
                                        descripcion,
                                        notas,
                                        estado,
                                        registra_fecha,
                                        registra_user,
                                        maximo_otorga,
                                        modifica_monto,
                                        modifica_diferencia,
                                        cod_cuenta,
                                        aplica_beneficiarios,
                                        aplica_parcial, 
                                        tipo_monetario,
                                        tipo_producto, 
                                        tipo,
                                        i_condicion_especial,
                                        i_morosidad,
                                        i_suspendidos,
                                        i_insolventes,
                                        i_cobro_judicial,
                                        Cod_Categoria, 
                                        Cod_Grupo, 
                                        VIGENCIA_MESES,
                                        PAGOS_MULTIPLES
                                        ) VALUES  ( 
                                        '{Beneficio.cod_beneficio}',
                                        '{Beneficio.descripcion}',
                                        '{Beneficio.notas}',
                                        '{Beneficio.estado}', 
                                         Getdate(),
                                        '{Beneficio.registra_user}',
                                         {Beneficio.maximo_otorga},
                                         {Beneficio.modifica_monto},
                                         {Beneficio.modifica_diferencia}, 
                                         '{Beneficio.cod_cuenta}', 
                                         {Beneficio.aplica_beneficiarios},
                                         {Beneficio.aplica_parcial},
                                         {Beneficio.tipo_monetario},
                                         {Beneficio.tipo_producto},
                                         '{Beneficio.tipo}',
                                         '{Beneficio.i_condicion_especial}',
                                          '{Beneficio.i_morosidad}',
                                          '{Beneficio.i_suspendidos}',
                                           '{Beneficio.i_insolventes}',
                                            '{Beneficio.i_cobro_judicial}',
                                         '{Beneficio.cod_categoria}',
                                          '{Beneficio.cod_grupo}', {Beneficio.vigencia_meses}, {Beneficio.pagos_multiples}) ";
                    connection.Execute(query);

                    query = $@"delete afi_Grupo_Beneficio where cod_beneficio = '{Beneficio.cod_beneficio}'";
                    connection.Execute(query);

                    query = $@"insert into afi_Grupo_Beneficio (cod_beneficio,cod_grupo) values ('{Beneficio.cod_beneficio}', {Beneficio.cod_grupo} ) ";
                    connection.Execute(query);


                    // Registrar el cambio de vigencia
                    string detalleVigencia = $@"Inserta [{Beneficio.vigencia_meses} meses] de vigencia del Beneficio";
                    RegistrarBitacora(CodCliente, "Inserta-Web", detalleVigencia, Beneficio.cod_beneficio, Beneficio.registra_user);

                    // Registrar el cambio de estado
                    string detalleEstado = $@"Inserta [{Beneficio.estado}] de Estado";
                    RegistrarBitacora(CodCliente, "Inserta-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);

                    // Registrar si hay un cambio en 'notas' y registrar en la bitácora si es necesario
                    string detalleNotas = $@"Inserta notas {Beneficio.estado}";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);


                    // Registrar si hay un cambio en 'categoria' y registrar en la bitácora si es necesario
                    string detalleCategoria = $@"Inserta categoria [{Beneficio.cod_categoria}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);


                    // Registrar si hay un cambio en 'grupo' y registrar en la bitácora si es necesario
                    string detalleGrupo = $@"Inserta grupo [{Beneficio.cod_grupo}]";
                        RegistrarBitacora(CodCliente, "Actualiza-Web", detalleEstado, Beneficio.cod_beneficio, Beneficio.registra_user);
                    





                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El código de beneficio ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }

        public ErrorDto AfiBeneficios_Eliminar(int CodCliente, string Cod_Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete afi_beneficios where cod_beneficio = '{Cod_Beneficio}'";
                    connection.Execute(query);
                }
            }
            catch (Exception)
            {
                info.Code = -1;
                info.Description = "No se puede eliminar el beneficio, ya que tiene registros asociados";
            }
            return info;
        }

        private ErrorDto AfiBeneficioMontos_Insertar(int CodCliente, AfiBeneficioMontoData Monto)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Obtengo consecutivo
                    var QueryCons = $"select isnull(max(id_bene),0) + 1 as Secuencia  from afi_beneficio_montos where cod_beneficio = '{Monto.cod_beneficio}' ";
                    var Consecutivo = connection.QueryFirstOrDefault<int>(QueryCons);

                    var query = $@"insert afi_beneficio_montos(id_bene,cod_beneficio,inicio,corte,monto) values
                                     ({Consecutivo}, '{Monto.cod_beneficio}' ,{Monto.inicio},{Monto.corte},{Monto.monto})";
                    connection.Execute(query);

                    //Obtengo el ultimo id_bene
                    info.Description = Consecutivo.ToString();

                    // Crear el detalle para la bitácora
                    string detalleMonto = $@"El monto es [{Monto.monto}] y el plazo es [{Monto.inicio}-{Monto.corte}]";

                    // Llamar al método auxiliar para registrar en la bitácora
                    RegistrarBitacora(CodCliente,"Inserta-Web", detalleMonto, Monto.cod_beneficio, Monto.registra_user);


                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        private ErrorDto AfiBeneficioMonto_Actualziar(int CodCliente, AfiBeneficioMontoData Monto)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update afi_beneficio_montos set inicio = {Monto.inicio},corte = {Monto.corte},monto = {Monto.monto}
                                     where id_bene = {Monto.id_bene} and cod_beneficio = '{Monto.cod_beneficio}'";
                    connection.Execute(query);
                    info.Description = Monto.id_bene.ToString();
                    string detalleMonto = $@"El nuevo monto es [{Monto.monto}] y el plazo es Fecha Desde: {Monto.inicio} Días - Fecha Hasta: {Monto.corte} Días";
                    // Llamar al método auxiliar para registrar en la bitácora
                    RegistrarBitacora(CodCliente,"Act-Web", detalleMonto, Monto.cod_beneficio, Monto.registra_user);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto AfiBeneficioMontos_Eliminar(int CodCliente, int id_bene, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete afi_beneficio_montos where id_bene = {id_bene} and cod_beneficio = '{cod_beneficio}'";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto NombreCuenta_Obtener(int CodCliente, string cuenta)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select descripcion from CntX_Cuentas where COD_CUENTA = '{cuenta}' order by cod_cuenta";
                    info.Description = connection.QueryFirstOrDefault<string>(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto<List<AfiBeneListas>> AfiBeneCategoria_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_categoria as 'item', descripcion as 'descripcion' 
                                     From afi_bene_categorias  where Activo = 1 order by descripcion";
                    response.Result = connection.Query<AfiBeneListas>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<List<AfiBeneListas>> AfiBeneGrupos_Obtener(int CodCliente, string categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneListas>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" select COD_GRUPO as 'item', DESCRIPCION as 'descripcion'
                                        From AFI_BENE_GRUPOS   Where Cod_Categoria = '{categoria}'   and Estado = 1 order by DESCRIPCION ";
                    response.Result = connection.Query<AfiBeneListas>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto AfiBeneficioMontos_Guardar(int CodCliente, AfiBeneficioMontoData Monto)
        {
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            if (Monto.id_bene == 0)
            {
                info = AfiBeneficioMontos_Insertar(CodCliente, Monto);
            }
            else
            {
                info = AfiBeneficioMonto_Actualziar(CodCliente, Monto);
            }

            return info;
        }

        public ErrorDto<List<BitacoraBeneficioDTO>> BitacoraBeneficio_Obtener(int CodEmpresa, string Cod_Beneficio, int Consec, string? cod_grupo, string? cod_categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BitacoraBeneficioDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT * 
                                    FROM (
                                        SELECT B.ID_BITACORA, B.CONSEC, B.REGISTRO_FECHA, B.COD_BENEFICIO, B.REGISTRO_USUARIO, B.DETALLE, B.MOVIMIENTO
                                        FROM AFI_BENE_REGISTRO_BITACORA B
                                        WHERE B.COD_BENEFICIO = '{cod_grupo}' AND B.CONSEC = -2
                                        UNION ALL
                                        SELECT B.ID_BITACORA, B.CONSEC, B.REGISTRO_FECHA, B.COD_BENEFICIO, B.REGISTRO_USUARIO, B.DETALLE, B.MOVIMIENTO
                                        FROM AFI_BENE_REGISTRO_BITACORA B
                                        WHERE B.COD_BENEFICIO = '{Cod_Beneficio}' AND B.CONSEC = -1
                                        UNION ALL
                                        SELECT B.ID_BITACORA, B.CONSEC, B.REGISTRO_FECHA, B.COD_BENEFICIO, B.REGISTRO_USUARIO, B.DETALLE, B.MOVIMIENTO
                                        FROM AFI_BENE_REGISTRO_BITACORA B
                                        WHERE B.COD_BENEFICIO = '{cod_categoria}' AND B.CONSEC = -2
                                    ) T
                                    ORDER BY T.REGISTRO_FECHA DESC;";

                    response.Result = connection.Query<BitacoraBeneficioDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BitacoraBeneficio_Obtener: " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<List<AfiBeneFechaPagoData>> AfiBeneFechasPago_Obtener(int CodCliente, string Cod_Beneficio, int Periodo)
      {
          var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
          var response = new ErrorDto<List<AfiBeneFechaPagoData>>
          {
              Code = 0
          };
          try
          {
              using var connection = new SqlConnection(clienteConnString);
              {
                  var query = $"select COD_CATEGORIA from AFI_BENEFICIOS where COD_BENEFICIO = '{Cod_Beneficio}'";
                  string Cod_Categoria = connection.Query<string>(query).FirstOrDefault();
                  query = $"SELECT COUNT(*) FROM AFI_BENE_FECHA_PAGO_AUTOMATICO where COD_BENEFICIO = '{Cod_Beneficio}' and PERIODO = {Periodo}";
                  int existe = connection.Query<int>(query).FirstOrDefault();
                  if (existe > 0)
                  {
                      query = $@"select * from AFI_BENE_FECHA_PAGO_AUTOMATICO where COD_BENEFICIO = '{Cod_Beneficio}' and PERIODO = {Periodo}";
                  }
                  else
                  {
                      query = $@"SELECT 
                          n AS id_fecha_pago, 
                          '{Cod_Beneficio}' AS cod_beneficio, 
                          '{Cod_Categoria}' AS cod_categoria, 
                          EOMONTH(DATEFROMPARTS(YEAR(GETDATE()), n, 1)) AS fecha_corte, 
                          n as mes, 
                          {Periodo} AS periodo, 
                          0 as monto,
                          1 AS activo 
                      FROM 
                          (VALUES (1), (2), (3), (4), (5), (6), (7), (8), (9), (10), (11), (12)) AS meses(n);";
                  }
                  response.Result = connection.Query<AfiBeneFechaPagoData>(query).ToList();
              }
          }
          catch (Exception ex)
          {
              response.Code = -1;
              response.Description = ex.Message;
              response.Result = null;
          }
          return response;
      }
      
        public ErrorDto AfiBeneFechasPago_Guardar(int CodCliente, List<AfiBeneFechaPagoData> DataFechas, string Usuario)
      {
          var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
          var listaDatos = DataFechas;
          var response = new ErrorDto
          {
              Code = 0
          };
          try
          {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select monto from AFI_BENE_GRUPOS where COD_GRUPO IN (select COD_GRUPO from Afi_beneficios where COD_BENEFICIO = '{listaDatos[0].cod_beneficio}')";
                    float monto = connection.Query<float>(query).FirstOrDefault();

                    float sumaMonto = 0;
                    foreach (AfiBeneFechaPagoData item in DataFechas)
                    {
                        sumaMonto += item.monto;
                    }

                    if (monto != sumaMonto && listaDatos[0].cod_categoria != "B_RECO" && listaDatos[0].cod_categoria != "B_FENA")
                    {
                        response.Code = -1;
                        response.Description = "El monto total debe ser igual al monto del grupo asignado: " + monto;
                        return response;
                    }

                    query = $"DELETE FROM AFI_BENE_FECHA_PAGO_AUTOMATICO where COD_BENEFICIO = '{listaDatos[0].cod_beneficio}' and PERIODO  = {listaDatos[0].periodo}";
                    connection.Execute(query);
                    foreach (AfiBeneFechaPagoData dato in listaDatos)
                    {
                        DateTimeOffset fecha_corte = DateTimeOffset.Parse(dato.fecha_corte.ToString());
                        string fechacorte = fecha_corte.ToString("yyyy-MM-dd");
                        int activoValor = dato.activo ? 1 : 0;
                        query = $@"INSERT INTO AFI_BENE_FECHA_PAGO_AUTOMATICO 
            (cod_beneficio, cod_categoria, fecha_corte, activo, periodo, mes, registro_fecha, registro_usuario, monto) 
            VALUES('{dato.cod_beneficio}', '{dato.cod_categoria}', '{fechacorte} 05:00:00',
            {activoValor}, {dato.periodo}, {dato.mes}, GETDATE(), '{Usuario}', {dato.monto});";
                        response.Code = connection.Execute(query);
                        response.Description = "Registros guardados correctamente";
                    }
                }
            }
          catch (Exception ex)
          {
              response.Code = -1;
              response.Description = ex.Message;
          }
          return response;
      }

        private void RegistrarBitacora(int CodCliente, string movimiento, string detalle, string codBeneficio, string registraUser)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
            {
                EmpresaId = CodCliente,
                cod_beneficio = codBeneficio,
                consec = -1,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = registraUser
            });
        }
    }

}