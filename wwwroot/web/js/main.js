var Host = "http://192.168.1.13:8080";
var amountPerPage = 10;

var cur_page = 0;
var maxPage = 0;
var NewsData = {};
// page number: key (string) | list of objects: value

function onClickOptions() {
    console.log(window.location.href)
    if (window.location.href.endsWith('/') || window.location.href == Host) {
        window.location.href = "/options"

    } else {
        history.back()
    }
}

function handlePage() {
    // ◀ Page {page}
    //  Page {page} 
    // Page {page + 1} ▶ 
    let num = Number(cur_page)
    let prevPage = document.getElementById("prev-page")
    let curPage = document.getElementById("cur-page")
    let nextPage = document.getElementById("next-page")

    if (num > 1) {
        prevPage.innerHTML = " ◀ Page " + (num - 1)+" "

    } else if (num == 1) {
        prevPage.innerHTML = ""
    }
        
    curPage.innerHTML = " Page " + num + " "
       
    nextPage.innerHTML = " Page " + (num + 1)+" ▶ "
    
}

function showNews(data) {
    var newsDiv = document.getElementById("news")

    var newNewsDiv = document.createElement("div")
    newNewsDiv.className = "news-view"
    newNewsDiv.onclick = () => goToNews(data.id)

    
    let thumbnail = document.createElement("img")
    if (data.thumbnail_path.length) {
        thumbnail.className = "news-thumbnail"
        thumbnail.src = data.thumbnail_path.replace('\\\\', '/').slice(1)
    }

    newNewsDiv.appendChild(thumbnail)

    let title = document.createElement("p")
    title.className = "news-text"
    title.innerHTML = data.title
    newNewsDiv.appendChild(title)

    if (data.tags === null) {
        data.tags = ""
    }

    var splited = data.tags.split(';');

    var tagsDiv = document.createElement("div")
    tagsDiv.className = "news-tag-view"
    var count = 0;

    var tagsColumDiv = document.createElement("div")
    tagsColumDiv.className = "news-tag-view2"
    for (let i in splited) {
        let tagValue = splited[i]
        if (tagValue.length === 0) {
            continue
        }
        
        let tags = document.createElement("p")
        tags.className = "news-tags"
        tags.innerHTML = tagValue
        tagsColumDiv.appendChild(tags)
        
        count++;

        if (count % 5 === 0 || i == (splited.length - 1)) {
            tagsDiv.appendChild(tagsColumDiv)
            tagsColumDiv = document.createElement("div")
            tagsColumDiv.className = "news-tag-view2"
        }
    }

    newNewsDiv.appendChild(tagsDiv)

    // posted_by_Admin_username
    let author = document.createElement("p")
    author.className = "news-author"
    author.innerHTML = "Posted By: " + data.posted_by_Admin_username
    newNewsDiv.appendChild(author)

    let posted_on = document.createElement("p")
    posted_on.className = "news-author"
    posted_on.innerHTML = "Posted on: " + new Date(data.posted_on).toLocaleString()
    newNewsDiv.appendChild(posted_on)

    let isFav = document.createElement("p")
    isFav.className = "news-fav"
    isFav.innerHTML = data.isFav ? "⭐" : ""
    newNewsDiv.appendChild(isFav)


    newsDiv.appendChild(newNewsDiv)
}

function clearNews() {
    var newsDiv = document.getElementById("news")
    while (newsDiv.firstChild) {
        newsDiv.removeChild(newsDiv.lastChild);
    }
}

function showPage(_page) {
    let news = NewsData[String(_page)]
    for (let x in news) {
        showNews(news[x])
    }
}

async function submitSearch(search, tags, authors, page) {
    let formData = new FormData()
    formData.append("tags", tags)
    formData.append("search", search)
    formData.append("post_authors", authors)
    formData.append("page", page)
    formData.append("amount", amountPerPage)

    const res = await fetch(Host + "/news/search", {
        method: "POST",
        body: formData,
        redirect: "follow",
    })

    if (res.status !== 200) {
        alert("No success")
        return;
    }

    const data = await res.json();
    const News = data.News;
    if (News === null || News === undefined || News.length === 0) {
        alert("No news")
        return;
    }

    cur_page = String(page)
    if (Number(page) > Number(maxPage)) {
        maxPage = String(page)
    }

    NewsData[cur_page] = News
    handlePage()
    clearNews()
    showPage(cur_page)
}


async function goPageForward() {
    let num_cur_page = Number(cur_page)
    if (num_cur_page === Number(maxPage) && NewsData[maxPage].length < amountPerPage) {
        alert("There shouldn't be any other news, "+
           +"but if you want to check if there are really no other news then submit your search again.")
        return
    }
    let nextPage = num_cur_page + 1
    if (NewsData[String(nextPage)] !== undefined) {
        clearNews()
        cur_page = nextPage
        handlePage()
        showPage(cur_page)
        return
    }
    await submitSearch(document.getElementById("search_title").value, '', '', nextPage)
}

function goPageBackword() {
    clearNews()
    cur_page = Number(cur_page) - 1
    handlePage()
    showPage(cur_page)
}

function getCookie(cname) {
    let name = cname + "=";
    let ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

const searchCookieV = getCookie("Search.S")
const tagsCookieV = getCookie("Search.T")
const authorsCookieV = getCookie("Search.A")

if (searchCookieV.length > 0 || tagsCookieV.length > 0 || authorsCookieV.length > 0) {
    submitSearch(searchCookieV, tagsCookieV, authorsCookieV, 1)
    fetch(Host + '/resetsearch', {method: 'GET', redirect:'follow'})
}

function goToSearchPage() {
    window.location.href = "/search"
}

// let searchV = document.getElementById("search_title").value
async function submitHomeSearch() {
    await submitSearch(document.getElementById("search_title").value,
        "", "", 1)
}

async function saveSearch(search, tags, authors) {
    let formData = new FormData()
    formData.append('search', search)
    formData.append('tags', tags)
    formData.append('authors', authors)

    const res = await fetch(Host + "/savesearch", {
        method: "POST",
        body: formData,
        redirect: 'follow'
    })

    if (res.status !== 200) {
        alert("Error.")
        return
    }

    window.location.href = "/"
}


function goToNews(id) {
    window.location.href = 'news/'+id
}

