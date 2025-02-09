
async function getSavedNews() {
    
    const res = await fetch(Host +'/options/savednews', {
        method: "POST",
        redirect: "follow"
    })

    if (res.status !== 200) {
        return;
    }

    const data = await res.json()

}

getSavedNews()