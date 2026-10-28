import { NavLink } from "react-router-dom";
import "./NavBar.css";

const LINKS = [
    { to: "/", label: "Runs", end: true },
    { to: "/stats", label: "Stats", end: false },
    { to: "/cards", label: "Cards", end: false },
    { to: "/relics", label: "Relics", end: false },
    { to: "/upload", label: "Upload", end: false },
];

function NavBar() {
    return (
        <nav className="navbar">
            <div className="navbar-inner container">
                <div className="navbar-brand">Spire Tracker</div>
                <div className="navbar-links">
                    {LINKS.map(link => (
                        <NavLink
                            key={link.to}
                            to={link.to}
                            end={link.end}
                            className={({ isActive }) =>
                                "navbar-link" + (isActive ? " navbar-link--active" : "")
                            }
                        >
                            {link.label}
                        </NavLink>
                    ))}
                </div>
            </div>
        </nav>
    );
}

export default NavBar;
