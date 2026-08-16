<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Sign in – Flash Sales</title>
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link href="https://fonts.googleapis.com/css2?family=Syne:wght@700&family=DM+Sans:opsz,wght@9..40,300;9..40,400;9..40,500&display=swap" rel="stylesheet" />
  <link rel="stylesheet" href="${url.resourcesPath}/css/login.css" />
</head>
<body>
<div class="page">
  <div class="card">

    <!-- ── Brand ─────────────────────────────────────── -->
    <div class="brand-row">
      <div class="brand-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2.5"
             stroke-linecap="round" stroke-linejoin="round">
          <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/>
        </svg>
      </div>
      <span class="brand-name">Flash Sales</span>
    </div>

    <!-- ── Header ────────────────────────────────────── -->
    <div class="header">
      <h2 class="title">Welcome back</h2>
      <p class="sub">Sign in to your account to continue.</p>
    </div>

    <!-- ── Alert ─────────────────────────────────────── -->
    <#if message?has_content && (message.type != "warning" || !isAppInitiatedAction??)>
      <div class="alert alert-${message.type}">
        ${message.summary?no_esc}
      </div>
    </#if>

    <!-- ── Social login ───────────────────────────────── -->
    <#if social.providers?has_content>
      <#list social.providers as p>
        <a href="${p.loginUrl}" class="btn-social">
          <#if p.alias == "github" || p.providerId == "github">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 2C6.477 2 2 6.477 2 12c0 4.418 2.865 8.166 6.839 9.489.5.092.682-.217.682-.482 0-.237-.009-.868-.013-1.703-2.782.604-3.369-1.342-3.369-1.342-.454-1.155-1.11-1.462-1.11-1.462-.908-.62.069-.608.069-.608 1.003.07 1.531 1.03 1.531 1.03.892 1.529 2.341 1.087 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.11-4.555-4.943 0-1.091.39-1.984 1.029-2.683-.103-.253-.446-1.27.098-2.647 0 0 .84-.269 2.75 1.025A9.578 9.578 0 0 1 12 6.836a9.59 9.59 0 0 1 2.504.337c1.909-1.294 2.747-1.025 2.747-1.025.546 1.377.202 2.394.1 2.647.64.699 1.028 1.592 1.028 2.683 0 3.842-2.339 4.687-4.566 4.935.359.309.678.919.678 1.852 0 1.336-.012 2.415-.012 2.743 0 .267.18.578.688.48C19.138 20.163 22 16.418 22 12c0-5.523-4.477-10-10-10z"/>
            </svg>
          <#elseif p.alias == "google" || p.providerId == "google">
            <svg width="16" height="16" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
              <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
              <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
              <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"/>
              <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"/>
            </svg>
          <#else>
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"/>
              <path d="M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"/>
              <path d="M2 12h20"/>
            </svg>
          </#if>
          Continue with ${p.displayName}
        </a>
      </#list>

      <div class="divider">
        <span class="div-line"></span>
        <span class="div-label">or sign in with email</span>
        <span class="div-line"></span>
      </div>
    </#if>

    <!-- ── Login form ─────────────────────────────────── -->
    <form action="${url.loginAction}" method="post" class="form" novalidate>
      <input type="hidden" id="id-hidden-input" name="credentialId"
             value="${(auth.selectedCredential)!''}"/>

      <!-- Email / username -->
      <#if !(usernameHidden?? && usernameHidden)>
        <div class="field-wrapper <#if messagesPerField.existsError('username','password')>has-error</#if>">
          <label class="field-label" for="username">
            <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24"
                 fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round">
              <rect width="20" height="16" x="2" y="4" rx="2"/>
              <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/>
            </svg>
            Email
          </label>
          <div class="input-wrap">
            <svg class="input-icon" xmlns="http://www.w3.org/2000/svg" width="15" height="15"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round">
              <rect width="20" height="16" x="2" y="4" rx="2"/>
              <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/>
            </svg>
            <input
              type="text"
              id="username"
              name="username"
              value="${(login.username)!''}"
              autocomplete="email"
              autofocus
              <#if usernameEditDisabled?? && usernameEditDisabled>disabled</#if>
            />
          </div>
          <#if messagesPerField.existsError('username')>
            <span class="field-error">${messagesPerField.get('username')?no_esc}</span>
          </#if>
        </div>
      </#if>

      <!-- Password -->
      <div class="field-wrapper <#if messagesPerField.existsError('password')>has-error</#if>">
        <label class="field-label" for="password">
          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24"
               fill="none" stroke="currentColor" stroke-width="2"
               stroke-linecap="round" stroke-linejoin="round">
            <rect width="18" height="11" x="3" y="11" rx="2" ry="2"/>
            <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
          </svg>
          Password
        </label>
        <div class="input-wrap">
          <svg class="input-icon" xmlns="http://www.w3.org/2000/svg" width="15" height="15"
               viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
               stroke-linecap="round" stroke-linejoin="round">
            <rect width="18" height="11" x="3" y="11" rx="2" ry="2"/>
            <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
          </svg>
          <input type="password" id="password" name="password" autocomplete="current-password" />
          <button type="button" class="eye-btn" onclick="togglePassword()" tabindex="-1"
                  aria-label="Toggle password visibility">
            <svg id="eye-show" xmlns="http://www.w3.org/2000/svg" width="15" height="15"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round">
              <path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/>
              <circle cx="12" cy="12" r="3"/>
            </svg>
            <svg id="eye-hide" xmlns="http://www.w3.org/2000/svg" width="15" height="15"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round" style="display:none">
              <path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/>
              <path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/>
              <path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/>
              <line x1="2" x2="22" y1="2" y2="22"/>
            </svg>
          </button>
        </div>
        <#if messagesPerField.existsError('password')>
          <span class="field-error">${messagesPerField.get('password')?no_esc}</span>
        </#if>
      </div>

      <!-- Forgot password -->
      <#if realm.resetPasswordAllowed>
        <div class="form-extra">
          <a href="${url.loginResetCredentialsUrl}" class="forgot-link">Forgot password?</a>
        </div>
      </#if>

      <!-- Submit -->
      <button type="submit" class="btn-primary">
        Sign in
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24"
             fill="none" stroke="currentColor" stroke-width="2"
             stroke-linecap="round" stroke-linejoin="round">
          <path d="M5 12h14"/><path d="m12 5 7 7-7 7"/>
        </svg>
      </button>
    </form>

    <!-- ── Register link ──────────────────────────────── -->
    <#if realm.registrationAllowed>
      <p class="switch-text">
        Don't have an account?
        <a href="${url.registrationUrl}" class="switch-link">Register</a>
      </p>
    </#if>

  </div>
</div>

<script>
  function togglePassword() {
    var inp  = document.getElementById('password');
    var show = document.getElementById('eye-show');
    var hide = document.getElementById('eye-hide');
    if (inp.type === 'password') {
      inp.type       = 'text';
      show.style.display = 'none';
      hide.style.display = 'block';
    } else {
      inp.type       = 'password';
      show.style.display = 'block';
      hide.style.display = 'none';
    }
  }
</script>
</body>
</html>
