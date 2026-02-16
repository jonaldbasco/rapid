// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function triggerSurge() {
    const pill = document.getElementById("statusPill");
    if (!pill) return;

    pill.classList.remove("status-normal");
    pill.classList.add("status-surge");

    pill.replaceChildren();

    const icon = document.createElement("i");
    icon.className = "fa-solid fa-triangle-exclamation";
    icon.setAttribute("aria-hidden", true);

    pill.append(icon, document.createTextNode(" SURGE ACTIVE"));

    // persist state across postback
    document.getElementById("isSurgeActive").value = "true";
}