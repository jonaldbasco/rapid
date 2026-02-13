//function triggerSurge() {
//    const pill = document.getElementById("statusPill");
//    if (!pill) return;

//    pill.classList.remove("status-normal");
//    pill.classList.add("status-surge");

//    pill.replaceChildren();

//    const icon = document.createElement("i");
//    icon.className = "fa-solid fa-triangle-exclamation";
//    icon.setAttribute("aria-hidden", true);

//    pill.append(icon, document.createTextNode(" SURGE ACTIVE"));

//    // persist state across postback
//    document.getElementById("isSurgeActive").value = "true";
//}

//async function triggerSurge() {
//    try {
//        const res = await fetch('/Home/TriggerSurge', {
//            method: 'POST',
//            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
//            body: 'count=10'
//        });

//        const data = await res.json();
//        console.log('TriggerSurge response:', data);

//        if (!res.ok) {
//            throw new Error('TriggerSurge failed');
//        }

//    } catch (e) {
//        console.error(e);
//        alert('Failed to trigger surge. Check console.');
//    }
//}

// wwwroot/js/rapid-main.js

async function triggerSurge() {
    try {
        const res = await fetch('/Home/TriggerSurge', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'count=10'
        });

        if (!res.ok) {
            const text = await res.text();
            throw new Error(`TriggerSurge failed: ${res.status} ${text}`);
        }

        // Optional: update the header pill if you still want it
        const pill = document.getElementById("statusPill");
        if (pill) {
            pill.classList.remove("status-normal");
            pill.classList.add("status-surge");
            pill.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i> SURGE ACTIVE';
        }

        // ✅ This calls the workflow starter defined in Index.cshtml
        if (typeof window.activateSurgeWorkflow === 'function') {
            window.activateSurgeWorkflow();
        }

    } catch (e) {
        console.error(e);
        alert('Failed to trigger surge. Check DevTools console.');
    }
}