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

