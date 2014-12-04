﻿namespace Suave

module Cookie =
  open System
  open System.Text
  open System.Globalization

  open Types

  type CookieLife =
    | Session
    | MaxAge of TimeSpan

  type CookieError =
    | NoCookieFound of string (* cookie id *)
    | DecryptionError of Crypto.SecretboxDecryptionError

  /// Parse the cookie's name and data in the string into a dictionary.
  val parse_cookie : cookie_string:string -> HttpCookie

  module HttpRequest =

    /// Finds the cookies of the request, or an empty Map otherwise, if
    /// there are no cookies.
    val cookies : request:HttpRequest -> Map<string, HttpCookie>

  module HttpResult =

    val cookies : result:HttpResult -> Map<string, HttpCookie>

  val set_cookie : cookie:HttpCookie -> WebPart
  val unset_cookie : name:string -> WebPart

  /// Sets the cookies to the HttpResult
  val set_pair   : http_cookie:HttpCookie -> client_cookie:HttpCookie -> WebPart
  val unset_pair : http_cookie_name:string -> WebPart

  /// A DTO structure for passing the right parameters to the XXX_cookies functions
  /// in this module.
  type CookiesState =
    { server_key      : ServerKey
      cookie_name     : string
      user_state_key  : string
      relative_expiry : CookieLife
      secure          : bool }

  [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
  module CookiesState =

    val mk : server_key:ServerKey ->
             cookie_name:string ->
             user_state_key:string ->
             relative_expiry:CookieLife ->
             secure:bool ->
             CookiesState

  val generate_cookies : server_key:ServerKey ->
                         cookie_name:string ->
                         relative_expiry:CookieLife ->
                         secure:bool ->
                         plain_data:byte[] ->
                         HttpCookie * HttpCookie

  val read_cookies : key:ServerKey ->
                     cookie_name:string ->
                     ctx:HttpContext ->
                     Choice<HttpCookie * byte [], CookieError>

  val refresh_cookies : expiry:CookieLife ->
                        cookie:HttpCookie ->
                        WebPart

  val update_cookies :  csctx:CookiesState ->
                        f_plain_text : (byte [] option -> byte []) ->
                        WebPart
  
  val cookie_state : csctx:CookiesState ->
                     no_cookie :(unit -> Choice<byte [], WebPart>) ->
                     decryption_failure:(Crypto.SecretboxDecryptionError -> WebPart) ->
                     WebPart